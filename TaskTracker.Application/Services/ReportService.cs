using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
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
                    // Берём имя исполнителя из первой задачи группы (репозиторий грузит Assignee через Include)
                    var first = g.FirstOrDefault();
                    var assigneeName = first?.Assignee?.Name;

                    return new OverdueByAssigneeDto
                    {
                        Assignee = !string.IsNullOrWhiteSpace(assigneeName)
                            ? assigneeName
                            : $"User#{g.Key}",

                        OverdueCount = g.Count(),

                        // Детализация: только просроченные задачи
                        Tasks = g.Select(TaskDto.FromEntity).ToList()
                    };
                })
                // Чтобы выдача была стабильная: сначала у кого больше просроченных
                .OrderByDescending(x => x.OverdueCount)
                .ThenBy(x => x.Assignee)
                .ToList();

            return result;
        }
    }
}
