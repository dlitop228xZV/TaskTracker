using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            ITaskRepository taskRepository,
            AppDbContext context,
            ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException($"Задача с ID {id} не найдена");

            return task;
        }

        public async Task<List<TaskDto>> GetAllTasksAsync()
        {
            return await GetAllTasksAsync(null, null, null, null);
        }

        public async Task<List<TaskDto>> GetAllTasksAsync(int? assigneeId, DateTime? dueBefore, DateTime? dueAfter)
        {
            return await GetAllTasksAsync(assigneeId, dueBefore, dueAfter, null);
        }

        public async Task<List<TaskDto>> GetAllTasksAsync(int? assigneeId, DateTime? dueBefore, DateTime? dueAfter, List<int>? tagIds)
        {
            var tasks = await _taskRepository.GetAllAsync(assigneeId, dueBefore, dueAfter, tagIds);
            return tasks.Select(TaskDto.FromEntity).ToList();
        }

        public async Task<TaskItem> CreateTaskAsync(CreateTaskDto createDto)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == createDto.AssigneeId);
                if (!userExists)
                    throw new ArgumentException($"Пользователь с ID {createDto.AssigneeId} не найден");

                // Валидация тегов (если прислали)
                var tagIds = createDto.TagIds?.Distinct().ToList() ?? new List<int>();
                if (tagIds.Any())
                {
                    var foundCount = await _context.Tags.CountAsync(t => tagIds.Contains(t.Id));
                    if (foundCount != tagIds.Count)
                    {
                        var existing = await _context.Tags.Where(t => tagIds.Contains(t.Id)).Select(t => t.Id).ToListAsync();
                        var missing = tagIds.Except(existing).ToList();
                        throw new ArgumentException($"Теги с ID {string.Join(", ", missing)} не найдены");
                    }
                }

                var task = new TaskItem
                {
                    Title = createDto.Title,
                    Description = createDto.Description ?? "",
                    AssigneeId = createDto.AssigneeId,
                    DueDate = createDto.DueDate,
                    Priority = createDto.Priority,
                    Status = TaskItemStatus.New,
                    CreatedAt = DateTime.UtcNow
                };

                // Добавляем связи TaskTags (без удаления — новая задача)
                if (tagIds.Any())
                {
                    foreach (var id in tagIds)
                    {
                        task.TaskTags.Add(new TaskTag { TagId = id });
                    }
                }

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                // Вернём с навигациями
                return await _taskRepository.GetByIdAsync(task.Id) ?? task;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<TaskDto>> GetFilteredTasksAsync(
            string status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int> tagIds = null)
        {
            IQueryable<TaskItem> query = _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TaskItemStatus>(status, true, out var parsedStatus))
                    query = query.Where(t => t.Status == parsedStatus);
            }

            if (assigneeId.HasValue)
                query = query.Where(t => t.AssigneeId == assigneeId.Value);

            if (dueBefore.HasValue)
                query = query.Where(t => t.DueDate <= dueBefore.Value);

            if (dueAfter.HasValue)
                query = query.Where(t => t.DueDate >= dueAfter.Value);

            if (tagIds != null && tagIds.Any())
                query = query.Where(t => t.TaskTags.Any(tt => tagIds.Contains(tt.TagId)));

            var tasks = await query.ToListAsync();

            return tasks.Select(TaskDto.FromEntity).ToList();
        }

        public async Task<TaskItem> UpdateTaskAsync(int id, UpdateTaskDto updateDto)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var task = await _context.Tasks
                    .Include(t => t.TaskTags)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                    throw new KeyNotFoundException($"Задача с ID {id} не найдена");

                if (!string.IsNullOrWhiteSpace(updateDto.Title))
                {
                    if (updateDto.Title.Length < 3 || updateDto.Title.Length > 200)
                        throw new ArgumentException("Название должно быть от 3 до 200 символов");
                    task.Title = updateDto.Title;
                }

                if (updateDto.Description != null)
                    task.Description = updateDto.Description;

                if (updateDto.AssigneeId.HasValue)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.Id == updateDto.AssigneeId.Value);
                    if (!userExists)
                        throw new ArgumentException($"Пользователь с ID {updateDto.AssigneeId} не найден");
                    task.AssigneeId = updateDto.AssigneeId.Value;
                }

                if (updateDto.DueDate.HasValue)
                {
                    if (updateDto.DueDate.Value < task.CreatedAt)
                        throw new ArgumentException("Дата выполнения не может быть раньше даты создания");
                    task.DueDate = updateDto.DueDate.Value;
                }

                if (updateDto.Status.HasValue)
                {
                    var newStatus = updateDto.Status.Value;

                    if (newStatus == TaskItemStatus.Done && task.Status != TaskItemStatus.Done)
                        task.CompletedAt = DateTime.UtcNow;
                    else if (newStatus != TaskItemStatus.Done && task.Status == TaskItemStatus.Done)
                        task.CompletedAt = null;

                    task.Status = newStatus;
                }

                if (updateDto.Priority.HasValue)
                {
                    if (!Enum.IsDefined(typeof(TaskPriority), updateDto.Priority.Value))
                        throw new ArgumentException($"Некорректный приоритет: {updateDto.Priority}");

                    task.Priority = updateDto.Priority.Value;
                }

                if (updateDto.TagIds != null)
                {
                    var newTagIds = updateDto.TagIds.Distinct().ToList();

                    if (newTagIds.Any())
                    {
                        var foundCount = await _context.Tags.CountAsync(t => newTagIds.Contains(t.Id));
                        if (foundCount != newTagIds.Count)
                        {
                            var existing = await _context.Tags.Where(t => newTagIds.Contains(t.Id)).Select(t => t.Id).ToListAsync();
                            var missing = newTagIds.Except(existing).ToList();
                            throw new ArgumentException($"Теги с ID {string.Join(", ", missing)} не найдены");
                        }
                    }

                    // удалить старые
                    var existingLinks = await _context.TaskTags.Where(tt => tt.TaskId == task.Id).ToListAsync();
                    _context.TaskTags.RemoveRange(existingLinks);

                    // добавить новые
                    if (newTagIds.Any())
                    {
                        foreach (var tagId in newTagIds)
                        {
                            _context.TaskTags.Add(new TaskTag
                            {
                                TaskId = task.Id,
                                TagId = tagId
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // вернуть обновлённую с навигациями
                return await _taskRepository.GetByIdAsync(id) ?? task;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ChangeTaskStatusAsync(int taskId, string newStatus)
        {
            var task = await GetTaskByIdAsync(taskId);

            if (Enum.TryParse<TaskItemStatus>(newStatus, out var statusEnum))
            {
                if (statusEnum == TaskItemStatus.Done && task.Status != TaskItemStatus.Done)
                    task.CompletedAt = DateTime.UtcNow;
                else if (statusEnum != TaskItemStatus.Done && task.Status == TaskItemStatus.Done)
                    task.CompletedAt = null;

                task.Status = statusEnum;
                await _taskRepository.UpdateAsync(task);
                return true;
            }

            return false;
        }

        public async Task<List<TaskDto>> GetOverdueTasksAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return tasks.Where(t => t.IsOverdue).Select(TaskDto.FromEntity).ToList();
        }

        public async Task<int> GetTasksCountByStatusAsync(string status)
        {
            if (Enum.TryParse<TaskItemStatus>(status, out var statusEnum))
            {
                var tasks = await _taskRepository.GetAllAsync();
                return tasks.Count(t => t.Status == statusEnum);
            }
            throw new ArgumentException($"Некорректный статус: {status}");
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Задача с ID {TaskId} не найдена для удаления", id);
                    return false;
                }

                await _taskRepository.DeleteAsync(task);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении задачи ID {TaskId}: {ErrorMessage}",
                    id, ex.Message);
                throw new ApplicationException($"Ошибка при удалении задачи: {ex.Message}", ex);
            }
        }
    }
}