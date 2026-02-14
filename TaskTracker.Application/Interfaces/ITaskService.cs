using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItem> GetTaskByIdAsync(int id);

        Task<List<TaskDto>> GetAllTasksAsync();

        /// <summary>
        /// Получить список задач с опциональной фильтрацией по исполнителю и диапазону дедлайна.
        /// </summary>
        /// <param name="assigneeId">Фильтр по исполнителю</param>
        /// <param name="dueBefore">DueDate <= dueBefore</param>
        /// <param name="dueAfter">DueDate >= dueAfter</param>
        Task<List<TaskDto>> GetAllTasksAsync(int? assigneeId, DateTime? dueBefore, DateTime? dueAfter);

        Task<TaskItem> CreateTaskAsync(CreateTaskDto createDto);
        Task<List<TaskDto>> GetFilteredTasksAsync(
            string status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int> tagIds = null);

        Task<TaskItem> UpdateTaskAsync(int id, UpdateTaskDto updateDto);

        Task<bool> DeleteTaskAsync(int id);

        Task<bool> ChangeTaskStatusAsync(int taskId, string newStatus);
        Task<List<TaskDto>> GetOverdueTasksAsync();
        Task<int> GetTasksCountByStatusAsync(string status);
    }
}
