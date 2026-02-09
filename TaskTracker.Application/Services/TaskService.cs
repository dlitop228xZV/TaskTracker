using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context; // Для работы с тегами

        public TaskService(ITaskRepository taskRepository, AppDbContext context)
        {
            _taskRepository = taskRepository;
            _context = context;
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

            // Пока без тегов, добавим позже
            return await _taskRepository.AddAsync(task);
        }

        // Остальные методы - заглушки
        public Task<List<TaskDto>> GetFilteredTasksAsync(string status = null, int? assigneeId = null, DateTime? dueBefore = null, DateTime? dueAfter = null, List<int> tagIds = null)
            => Task.FromResult(new List<TaskDto>());

        public async Task<TaskItem> UpdateTaskAsync(int id, UpdateTaskDto updateDto)
        {
            // 1. Найти задачу
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException($"Задача с ID {id} не найдена");

            // 2. Обновить основные поля
            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                task.Title = updateDto.Title;

            if (updateDto.Description != null) // Разрешаем пустое описание
                task.Description = updateDto.Description;

            if (updateDto.AssigneeId.HasValue)
                task.AssigneeId = updateDto.AssigneeId.Value;

            if (updateDto.DueDate.HasValue)
                task.DueDate = updateDto.DueDate.Value;

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
                    throw new ArgumentException($"Некорректный статус: {updateDto.Status}");
                }
            }

            // 4. Обновить приоритет
            if (updateDto.Priority.HasValue)
            {
                if (Enum.TryParse<TaskPriority>(updateDto.Priority.ToString(), out var priority))
                {
                    task.Priority = priority;
                }
                else
                {
                    throw new ArgumentException($"Некорректный приоритет: {updateDto.Priority}");
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


        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return false;

            await _taskRepository.DeleteAsync(id);
            return true;
        }

        public Task<bool> ChangeTaskStatusAsync(int taskId, string newStatus)
            => Task.FromResult(true);

        public Task<List<TaskDto>> GetOverdueTasksAsync()
            => Task.FromResult(new List<TaskDto>());

        public Task<int> GetTasksCountByStatusAsync(string status)
            => Task.FromResult(0);
    }
}