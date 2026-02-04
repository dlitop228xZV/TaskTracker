using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItem> GetTaskByIdAsync(int id);
        Task<List<TaskDto>> GetAllTasksAsync();
        Task<List<TaskDto>> GetFilteredTasksAsync(
            string status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int> tagIds = null);
        Task<TaskItem> CreateTaskAsync(CreateTaskDto createDto);
        Task<TaskItem> UpdateTaskAsync(int id, UpdateTaskDto updateDto);
        Task<bool> DeleteTaskAsync(int id);

        // Бизнес-логика
        Task<bool> ChangeTaskStatusAsync(int taskId, string newStatus);
        Task<List<TaskDto>> GetOverdueTasksAsync();
        Task<int> GetTasksCountByStatusAsync(string status);
    }
}