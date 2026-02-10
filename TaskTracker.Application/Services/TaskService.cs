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
            var tasks = await _taskRepository.GetAllAsync();
            return tasks.Select(TaskDto.FromEntity).ToList();
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
                Description = createDto.Description ?? "",
                AssigneeId = createDto.AssigneeId,
                DueDate = createDto.DueDate,
                Priority = createDto.Priority,
                Status = TaskItemStatus.New,
                CreatedAt = DateTime.UtcNow
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
            var tasks = await _taskRepository.GetFilteredAsync(
                status, assigneeId, dueBefore, dueAfter, tagIds);

            return tasks.Select(TaskDto.FromEntity).ToList();
        }

        public async Task<TaskItem> UpdateTaskAsync(int id, UpdateTaskDto updateDto)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            // 1. Найти задачу
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
            if (updateDto.Status.HasValue)
            {
                var newStatus = updateDto.Status.Value;

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

            // 4. Обновить приоритет
            if (updateDto.Priority.HasValue)
            {
                if (Enum.IsDefined(typeof(TaskPriority), updateDto.Priority.Value))
                {
                    task.Priority = updateDto.Priority.Value;
                }
                else
                {
                    throw new ArgumentException($"Некорректный приоритет: {updateDto.Priority}. " +
                        $"Допустимые значения: {string.Join(", ", Enum.GetNames(typeof(TaskPriority)))}");
                }
            }

            // 5. Обновить теги
            if (updateDto.TagIds != null)
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

            // 6. Сохранить изменения
            await _taskRepository.UpdateAsync(task);
            await _context.SaveChangesAsync();

            // 7. Вернуть обновлённую задачу с загруженными связями
            return await _taskRepository.GetByIdAsync(id);
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

        /// <summary>
        /// Удаляет задачу по ID (жёсткое удаление)
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <returns>True если удаление успешно, false если задача не найдена</returns>
        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                // Проверяем существование задачи
                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Задача с ID {TaskId} не найдена для удаления", id);
                    return false;
                }

                _logger.LogInformation("Начинаем удаление задачи ID {TaskId}", id);

                // Удаляем задачу (в репозитории уже должна быть логика удаления связанных TaskTags)
                await _taskRepository.DeleteAsync(task);

                _logger.LogInformation("Задача ID {TaskId} успешно удалена", id);
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