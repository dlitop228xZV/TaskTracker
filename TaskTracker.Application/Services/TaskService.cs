using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly AppDbContext _context;

        public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(
            string? status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int>? tagIds = null)
        {
            return await _taskRepository.GetAllAsync(status, assigneeId, dueBefore, dueAfter, tagIds);
        }

        public async Task<TaskDto> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetByIdWithDetailsAsync(id);

            if (task == null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found");
            }

            // Преобразование TaskItem -> TaskDto
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee?.Name ?? "Unknown",
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                Status = task.Status,
                Priority = task.Priority,
                Tags = task.TaskTags?
                    .Select(tt => tt.Tag?.Name)
                    .Where(name => name != null)
                    .ToList() ?? new List<string>()
            };
        }

        public async Task<TaskItem> CreateTaskAsync(CreateTaskDto createDto)
        {
            // Проверка существования пользователя
            var userExists = await _context.Users.AnyAsync(u => u.Id == createDto.AssigneeId);
            if (!userExists)
                throw new ArgumentException($"Пользователь с ID {createDto.AssigneeId} не найден");

            var task = new TaskItem
            {
                Title = createDto.Title,
                Description = createDto.Description,
                AssigneeId = createDto.AssigneeId,
                DueDate = createDto.DueDate,
                Priority = createDto.Priority,
                Status = (TaskStatus)TaskItemStatus.New // Используем enum значение
            };

            // Добавление тегов при создании
            if (createDto.TagIds?.Any() == true)
            {
                var tags = await _context.Tags
                    .Where(t => createDto.TagIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var tag in tags)
                {
                    task.TaskTags.Add(new TaskTag
                    {
                        TagId = tag.Id
                    });
                }
            }

            return await _taskRepository.AddAsync(task);
        }

        public async Task<List<TaskDto>> GetFilteredTasksAsync(
            string status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int> tagIds = null)
        {
            var tasks = await _taskRepository.GetAllAsync();

            var filtered = tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TaskItemStatus>(status, out var statusEnum))
                {
                    filtered = filtered.Where(t => t.Status == statusEnum);
                }
            }

            if (assigneeId.HasValue)
            {
                filtered = filtered.Where(t => t.AssigneeId == assigneeId.Value);
            }

            if (dueBefore.HasValue)
            {
                filtered = filtered.Where(t => t.DueDate <= dueBefore.Value);
            }

            if (dueAfter.HasValue)
            {
                filtered = filtered.Where(t => t.DueDate >= dueAfter.Value);
            }

            if (tagIds?.Any() == true)
            {
                filtered = filtered.Where(t =>
                    t.TaskTags.Any(tt => tagIds.Contains(tt.TagId)));
            }

            return filtered.Select(TaskDto.FromEntity).ToList();
        }

            return createdTask;
        }

        public async Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateDto)
        {
            // 1. Найти задачу (404 если не найдена)
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException($"Задача с ID {id} не найдена");

            // 2. Валидация данных
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

            // 3. Обновить статус с логикой CompletedAt
            if (!string.IsNullOrWhiteSpace(updateDto.Status))
            {
                if (Enum.TryParse<TaskItemStatus>(updateDto.Status, out var newStatus))
                {
                    // Если переводим в Done и задача ещё не была Done
                    if (newStatus == TaskItemStatus.Done && task.Status != TaskItemStatus.Done)
                    {
                        task.CompletedAt = DateTime.UtcNow;
                    }
                    // Если убираем из Done
                    else if (newStatus != TaskItemStatus.Done && task.Status == TaskItemStatus.Done)
                    {
                        task.CompletedAt = null;
                    }

                    task.Status = newStatus;
                }
                else
                {
                    throw new ArgumentException($"Некорректный статус: {updateDto.Status}. Допустимые значения: New, InProgress, Done");
                }
            }

            // 2. Проверить существование исполнителя
            var userExists = await _userRepository.ExistsAsync(updateDto.AssigneeId);
            if (!userExists)
            {
                if (updateDto.Priority >= 1 && updateDto.Priority <= 3)
                {
                    task.Priority = (TaskPriority)updateDto.Priority.Value;
                }
                else
                {
                    throw new ArgumentException($"Некорректный приоритет: {updateDto.Priority}. Допустимые значения: 1 (Low), 2 (Medium), 3 (High)");
                }
            }

            // 3. Обновить задачу
            task.Update(
                updateDto.Title,
                updateDto.Description,
                updateDto.AssigneeId,
                updateDto.DueDate,
                updateDto.Priority,
                updateDto.Status
            );

            // 4. Сохранить изменения задачи
            await _taskRepository.UpdateAsync(task);

            // 5. Обновить связи с тегами (если переданы TagIds)
            if (updateDto.TagIds != null && updateDto.TagIds.Any())
            {
                // Удалить старые связи
                var existingTaskTags = await _context.TaskTags
                    .Where(tt => tt.TaskId == task.Id)
                    .ToListAsync();
                _context.TaskTags.RemoveRange(existingTaskTags);

                // Добавить новые связи
                if (updateDto.TagIds.Any())
                {
                    var tags = await _context.Tags
                        .Where(t => updateDto.TagIds.Contains(t.Id))
                        .ToListAsync();

                    // Проверка существования всех тегов
                    var notFoundIds = updateDto.TagIds.Except(tags.Select(t => t.Id)).ToList();
                    if (notFoundIds.Any())
                        throw new ArgumentException($"Теги с ID {string.Join(", ", notFoundIds)} не найдены");

                    foreach (var tag in tags)
                    {
                        _context.TaskTags.Add(new TaskTag
                        {
                            TaskId = task.Id,
                            TagId = tag.Id
                        });
                    }
                }
            }

            // 6. Вернуть обновленную задачу как DTO
            return await GetTaskByIdAsync(id);
        }

        public async Task DeleteTaskAsync(int id)
        {
            await _taskRepository.DeleteAsync(id);
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var tasks = await _taskRepository.GetAllAsync();
            var now = DateTime.Now;

            var overdueTasks = tasks
                .Where(t => t.DueDate < now && t.Status != (TaskStatus)TaskItemStatus.Done)
                .GroupBy(t => t.Assignee?.Name ?? "Unknown")
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => $"Task #{t.Id}: {t.Title}").ToList()
                );

            return overdueTasks;
        }

        public async Task<bool> ChangeTaskStatusAsync(int taskId, string newStatus)
        {
            var task = await GetTaskByIdAsync(taskId);

            if (Enum.TryParse<TaskItemStatus>(newStatus, out var statusEnum))
            {
                // Если переводим в Done
                if (statusEnum == TaskItemStatus.Done && task.Status != TaskItemStatus.Done)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }
                // Если убираем из Done
                else if (statusEnum != TaskItemStatus.Done && task.Status == TaskItemStatus.Done)
                {
                    task.CompletedAt = null;
                }

                task.Status = statusEnum;
                await _taskRepository.UpdateAsync(task);
                return true;
            }

            return false;
        }

        public async Task<List<TaskDto>> GetOverdueTasksAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            var overdueTasks = tasks
                .Where(t => t.IsOverdue)
                .Select(TaskDto.FromEntity)
                .ToList();
            return overdueTasks;
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
    }
}