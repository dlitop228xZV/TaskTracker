using TaskTracker.Domain.Entities;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<List<TaskItem>> GetAllAsync();

    /// <summary>
    /// Получить все задачи с опциональной фильтрацией по исполнителю.
    /// Старый метод <see cref="GetAllAsync()"/> сохранён для обратной совместимости.
    /// </summary>
    /// <param name="assigneeId">Id исполнителя. Если null — возвращаются все задачи.</param>
    Task<List<TaskItem>> GetAllAsync(int? assigneeId);

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
