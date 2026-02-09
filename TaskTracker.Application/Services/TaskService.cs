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
        private readonly IUserRepository _userRepository;

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
            // Проверить существование исполнителя
            var userExists = await _userRepository.ExistsAsync(createDto.AssigneeId);
            if (!userExists)
            {
                throw new ArgumentException($"User with id {createDto.AssigneeId} not found");
            }

            var task = new TaskItem
            {
                Title = createDto.Title,
                Description = createDto.Description,
                AssigneeId = createDto.AssigneeId,
                DueDate = createDto.DueDate,
                Priority = createDto.Priority,
                Status = (TaskStatus)TaskItemStatus.New // Используем enum значение
            };

            var createdTask = await _taskRepository.AddAsync(task);

            // Добавить теги, если есть
            if (createDto.TagIds != null && createDto.TagIds.Any())
            {
                await _taskRepository.UpdateTaskTagsAsync(createdTask.Id, createDto.TagIds);
            }

            return createdTask;
        }

        public async Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateDto)
        {
            // 1. Найти задачу (404 если не найдена)
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found");
            }

            // 2. Проверить существование исполнителя
            var userExists = await _userRepository.ExistsAsync(updateDto.AssigneeId);
            if (!userExists)
            {
                throw new ArgumentException($"User with id {updateDto.AssigneeId} not found");
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
                await _taskRepository.UpdateTaskTagsAsync(id, updateDto.TagIds);
            }

            // 6. Вернуть обновленную задачу как DTO
            return await GetTaskByIdAsync(id);
        }

        public async Task DeleteTaskAsync(int id)
        {
            await _taskRepository.DeleteAsync(id);
        }

        // Методы для отчётов (заглушки)
        public async Task<Dictionary<string, int>> GetStatusSummaryAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return tasks
                .GroupBy(t => t.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, List<string>>> GetOverdueTasksByAssigneeAsync()
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

        public async Task<double> GetAverageCompletionTimeAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            var completedTasks = tasks
                .Where(t => t.Status == (TaskStatus)TaskItemStatus.Done && t.CompletedAt.HasValue)
                .ToList();

            if (!completedTasks.Any())
            {
                return 0;
            }

            var averageDays = completedTasks
                .Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalDays);

            return Math.Round(averageDays, 2);
        }
    }
}