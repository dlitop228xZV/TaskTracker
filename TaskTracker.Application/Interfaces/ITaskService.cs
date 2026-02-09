using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces
{
    public interface ITaskService
    {
        // Методы для задач
        Task<IEnumerable<TaskItem>> GetAllTasksAsync(
            string? status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int>? tagIds = null);

        Task<TaskDto> GetTaskByIdAsync(int id);
        Task<TaskItem> CreateTaskAsync(CreateTaskDto createDto);
        Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateDto);
        Task DeleteTaskAsync(int id);

        // Методы для отчётов
        Task<Dictionary<string, int>> GetStatusSummaryAsync();
        Task<Dictionary<string, List<string>>> GetOverdueTasksByAssigneeAsync();
        Task<double> GetAverageCompletionTimeAsync();
    }
}