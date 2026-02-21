using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItem> GetTaskByIdAsync(int id);

        // Старый метод — НЕ УДАЛЯЕМ
        Task<List<TaskDto>> GetAllTasksAsync();

        // Старый расширенный метод — НЕ УДАЛЯЕМ (если у тебя уже был)
        Task<List<TaskDto>> GetAllTasksAsync(int? assigneeId, DateTime? dueBefore, DateTime? dueAfter);

        /// <summary>
        /// Получить список задач с опциональной фильтрацией по исполнителю, диапазону дедлайна и тегам.
        /// tagIds: список id тегов; задача подходит если имеет хотя бы один из них.
        /// </summary>
        Task<List<TaskDto>> GetAllTasksAsync(
            int? assigneeId,
            DateTime? dueBefore,
            DateTime? dueAfter,
            List<int>? tagIds);

        Task<TaskItem> CreateTaskAsync(CreateTaskDto createDto);

        // Старый метод фильтрации — НЕ УДАЛЯЕМ
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