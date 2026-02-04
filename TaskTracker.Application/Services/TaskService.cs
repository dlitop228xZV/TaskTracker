using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services
{
    public class TaskService : ITaskService
    {
        private List<TaskItem> _tasks = new();  // временно в памяти

        public Task<TaskItem> GetTaskByIdAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(task);
        }

        public Task<List<TaskDto>> GetAllTasksAsync()
        {
            var dtos = _tasks.Select(t => TaskDto.FromEntity(t)).ToList();
            return Task.FromResult(dtos);
        }

        public Task<TaskItem> CreateTaskAsync(CreateTaskDto createDto)
        {
            var task = new TaskItem
            {
                Id = _tasks.Count + 1,
                Title = createDto.Title,
                Description = createDto.Description,
                AssigneeId = createDto.AssigneeId,
                DueDate = createDto.DueDate,
                Priority = createDto.Priority,
                Status = "New",
                CreatedAt = DateTime.UtcNow
            };

            _tasks.Add(task);
            return Task.FromResult(task);
        }

        // Остальные методы - заглушки
        public Task<List<TaskDto>> GetFilteredTasksAsync(string status = null, int? assigneeId = null, DateTime? dueBefore = null, DateTime? dueAfter = null, List<int> tagIds = null)
            => Task.FromResult(new List<TaskDto>());

        public Task<TaskItem> UpdateTaskAsync(int id, UpdateTaskDto updateDto)
            => Task.FromResult<TaskItem>(null);

        public Task<bool> DeleteTaskAsync(int id)
            => Task.FromResult(true);

        public Task<bool> ChangeTaskStatusAsync(int taskId, string newStatus)
            => Task.FromResult(true);

        public Task<List<TaskDto>> GetOverdueTasksAsync()
            => Task.FromResult(new List<TaskDto>());

        public Task<int> GetTasksCountByStatusAsync(string status)
            => Task.FromResult(0);
    }
}