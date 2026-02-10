using TaskTracker.Domain.Entities;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem> AddAsync(TaskItem entity);
    Task UpdateAsync(TaskItem entity);
    Task DeleteAsync(TaskItem entity);

    Task<List<TaskItem>> GetFilteredAsync(
        string status = null,
        int? assigneeId = null,
        DateTime? dueBefore = null,
        DateTime? dueAfter = null,
        List<int> tagIds = null);
}
