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

            // Группировка по EffectiveStatus, учитывая IsOverdue
            var result = tasks
                .GroupBy(t => t.IsOverdue ? "Overdue" : t.Status.ToString())
                .Select(g => new StatusSummaryItemDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Status) // стабильный вывод
                .ToList();

            return result;
        }
    }
}
