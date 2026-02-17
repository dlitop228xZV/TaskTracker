using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Сводка по статусам задач (включая Overdue).
        /// </summary>
        /// <remarks>
        /// Пример ответа:
        /// [
        ///   {"status":"New","count":5},
        ///   {"status":"InProgress","count":3},
        ///   {"status":"Done","count":12},
        ///   {"status":"Overdue","count":2}
        /// ]
        /// </remarks>
        [HttpGet("status-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Сводка задач по статусам", Description = "Группировка по EffectiveStatus с учётом Overdue.")]
        [SwaggerResponse(200, "Успешный запрос", typeof(List<StatusSummaryItemDto>))]
        public async Task<ActionResult<List<StatusSummaryItemDto>>> GetStatusSummary()
        {
            var summary = await _reportService.GetStatusSummaryAsync();
            return Ok(summary);
        }

        /// <summary>
        /// Просроченные задачи по исполнителям (группировка + детализация).
        /// </summary>
        /// <remarks>
        /// Пример ответа:
        /// [
        ///   {
        ///     "assignee": "Иванов И.И.",
        ///     "overdueCount": 2,
        ///     "tasks": [ { ...taskDto... }, { ...taskDto... } ]
        ///   }
        /// ]
        /// </remarks>
        [HttpGet("overdue-by-assignee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Просроченные задачи по исполнителям", Description = "Фильтрует IsOverdue=true, группирует по исполнителям, возвращает детализацию.")]
        [SwaggerResponse(200, "Успешный запрос", typeof(List<OverdueByAssigneeDto>))]
        public async Task<ActionResult<List<OverdueByAssigneeDto>>> GetOverdueByAssignee()
        {
            var result = await _reportService.GetOverdueByAssigneeAsync();
            return Ok(result);
        }
    }
}
