using TaskTracker.Domain.Entities;

namespace TaskTracker.Domain.Interfaces
{
    public interface ITaskRepository : IRepository<TaskItem>
    {
        Task<List<TaskItem>> GetFilteredAsync(
            string status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int> tagIds = null);
    }
}