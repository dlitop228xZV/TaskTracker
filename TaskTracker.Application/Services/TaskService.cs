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

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            // Репозиторий должен сам делать Include
            return await _taskRepository.GetByIdAsync(id);
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
            // Найти задачу, обновить, сохранить
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return null;

            if (!string.IsNullOrEmpty(updateDto.Title))
                task.Title = updateDto.Title;

            if (!string.IsNullOrEmpty(updateDto.Description))
                task.Description = updateDto.Description;

            if (updateDto.AssigneeId.HasValue)
                task.AssigneeId = updateDto.AssigneeId.Value;

            if (updateDto.DueDate.HasValue)
                task.DueDate = updateDto.DueDate.Value;

            if (!string.IsNullOrEmpty(updateDto.Status))
            {
                if (Enum.TryParse<TaskItemStatus>(updateDto.Status, out var statusEnum))
                    task.Status = statusEnum;
            }

            if (updateDto.Priority.HasValue)
            {
                if (Enum.TryParse<TaskPriority>(updateDto.Priority.ToString(), out var priorityEnum))
                    task.Priority = priorityEnum;
            }

            await _taskRepository.UpdateAsync(task);
            return task;
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