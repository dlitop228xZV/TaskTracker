using TaskTracker.Domain.Entities;

namespace TaskTracker.Domain.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync(
            string? status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int>? tagIds = null);

        Task<TaskItem?> GetByIdAsync(int id);
        Task<TaskItem?> GetByIdWithDetailsAsync(int id);
        Task<TaskItem> AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(int id);

        // Методы для работы с тегами
        Task UpdateTaskTagsAsync(int taskId, List<int> tagIds);
        Task SaveChangesAsync();
        Task<TaskEntity?> GetTaskByIdAsync(int id);
    }
}