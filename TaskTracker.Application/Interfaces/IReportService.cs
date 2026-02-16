using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces
{
    public interface IReportService
    {
        /// <summary>
        /// Сводка по статусам задач с учётом Overdue (computed, не хранится в БД).
        /// Группируем по EffectiveStatus: Overdue или Status.ToString().
        /// </summary>
        Task<List<StatusSummaryItemDto>> GetStatusSummaryAsync();
    }
}
