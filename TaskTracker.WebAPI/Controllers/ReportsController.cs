using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly ITaskRepository _taskRepository;

        public ReportService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<List<StatusSummaryItemDto>> GetStatusSummaryAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();

            var result = tasks
                .GroupBy(t => t.IsOverdue ? "Overdue" : t.Status.ToString())
                .Select(g => new StatusSummaryItemDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Status)
                .ToList();

            return result;
        }

        public async Task<List<OverdueByAssigneeDto>> GetOverdueByAssigneeAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();

            var overdue = tasks.Where(t => t.IsOverdue).ToList();

            var result = overdue
                .GroupBy(t => t.AssigneeId)
                .Select(g =>
                {
                    var first = g.FirstOrDefault();
                    var assigneeName = first?.Assignee?.Name;

                    return new OverdueByAssigneeDto
                    {
                        Assignee = !string.IsNullOrWhiteSpace(assigneeName)
                            ? assigneeName
                            : $"User#{g.Key}",
                        OverdueCount = g.Count(),
                        Tasks = g.Select(TaskDto.FromEntity).ToList()
                    };
                })
                .OrderByDescending(x => x.OverdueCount)
                .ThenBy(x => x.Assignee)
                .ToList();

            return result;
        }

        public async Task<double?> GetAvgCompletionTimeAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();

            var doneTasks = tasks
                .Where(t => t.Status == TaskItemStatus.Done && t.CompletedAt.HasValue)
                .ToList();

            if (!doneTasks.Any())
                return null;

            var avgDays = doneTasks
                .Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalDays);

            return avgDays;
        }
    }
}