using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/reports/status-summary
        [HttpGet("status-summary")]
        public async Task<IActionResult> GetStatusSummary()
        {
            var summary = new
            {
                New = await _context.Tasks.CountAsync(t => t.Status == (TaskStatus)TaskItemStatus.New),
                InProgress = await _context.Tasks.CountAsync(t => t.Status == (TaskStatus)TaskItemStatus.InProgress),
                Done = await _context.Tasks.CountAsync(t => t.Status == (TaskStatus)TaskItemStatus.Done),
                Overdue = await _context.Tasks.CountAsync(t =>
                    t.Status != (TaskStatus)TaskItemStatus.Done && t.DueDate.Date < DateTime.UtcNow.Date)
            };

            return Ok(summary);
        }
    }
}