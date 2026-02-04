using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces
{
    public interface IReportService
    {
        Task<Dictionary<string, int>> GetStatusSummaryAsync();
        Task<Dictionary<string, List<TaskDto>>> GetOverdueTasksByAssigneeAsync();
        Task<double?> GetAverageCompletionTimeInDaysAsync();
    }
}