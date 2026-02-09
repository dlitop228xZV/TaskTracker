using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
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

        public async Task DeleteAsync(int id)
        {
            var task = await GetByIdAsync(id);
            if (task != null)
            {
                // Удаляем связи с тегами
                var taskTags = await _context.TaskTags
                    .Where(tt => tt.TaskId == id)
                    .ToListAsync();
                _context.TaskTags.RemoveRange(taskTags);

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public Task<List<TaskItem>> GetFilteredAsync(
            string status = null,
            int? assigneeId = null,
            DateTime? dueBefore = null,
            DateTime? dueAfter = null,
            List<int> tagIds = null)
            => throw new NotImplementedException();
    }
}