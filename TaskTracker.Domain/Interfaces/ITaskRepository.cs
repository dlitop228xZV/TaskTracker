using TaskTracker.Domain.Entities;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<List<TaskItem>> GetAllAsync();

    /// <summary>
    /// Получить список задач с опциональной фильтрацией по исполнителю и диапазону дедлайна.
    /// </summary>
    Task<List<TaskItem>> GetAllAsync(int? assigneeId, DateTime? dueBefore, DateTime? dueAfter);

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
