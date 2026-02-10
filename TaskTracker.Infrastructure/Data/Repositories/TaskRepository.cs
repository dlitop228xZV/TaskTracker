using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Data.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .ToListAsync();
        }

        public IQueryable<TaskItem> GetAll()
        {
            return _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .AsQueryable();
        }

        public async Task<List<TaskItem>> FindAsync(System.Linq.Expressions.Expression<Func<TaskItem, bool>> predicate)
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<TaskItem> AddAsync(TaskItem entity)
        {
            _context.Tasks.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(TaskItem entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TaskItem entity)
        {
            // Удаляем связи с тегами
            var taskTags = await _context.TaskTags
                .Where(tt => tt.TaskId == entity.Id)
                .ToListAsync();
            _context.TaskTags.RemoveRange(taskTags);

            _context.Tasks.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await GetByIdAsync(id);
            if (task == null)
                return false;

            await DeleteAsync(task);
            return true;
        }

        public async Task<List<TaskItem>> GetFilteredAsync(
            string status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int> tagIds = null)
        {
            var query = _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TaskItemStatus>(status, out var statusEnum))
                {
                    query = query.Where(t => t.Status == statusEnum);
                }
            }

            if (assigneeId.HasValue)
            {
                query = query.Where(t => t.AssigneeId == assigneeId.Value);
            }

            if (dueBefore.HasValue)
            {
                query = query.Where(t => t.DueDate <= dueBefore.Value);
            }

            if (dueAfter.HasValue)
            {
                query = query.Where(t => t.DueDate >= dueAfter.Value);
            }

            if (tagIds?.Any() == true)
            {
                query = query.Where(t =>
                    t.TaskTags.Any(tt => tagIds.Contains(tt.TagId)));
            }

            return await query.ToListAsync();
        }
    }
}