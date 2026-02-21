using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Enums;

namespace TaskTracker.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Получить список задач с возможностью фильтрации.
        /// </summary>
        /// <remarks>
        /// Примеры:
        /// GET /api/tasks
        /// GET /api/tasks?assigneeId=2
        /// GET /api/tasks?dueBefore=2026-02-20
        /// GET /api/tasks?dueAfter=2026-02-01&amp;dueBefore=2026-02-20
        /// GET /api/tasks?tagIds=1&amp;tagIds=3
        /// GET /api/tasks?assigneeId=2&amp;tagIds=1
        /// GET /api/tasks?status=InProgress&amp;assigneeId=2&amp;dueBefore=2026-02-20&amp;tagIds=1&amp;tagIds=3
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Получить все задачи", Description = "Возвращает список задач. Поддерживает фильтры в query.")]
        [SwaggerResponse(200, "Успешный запрос", typeof(List<TaskDto>))]
        public async Task<ActionResult<List<TaskDto>>> GetTasks(
            [FromQuery] TaskItemStatus? status,
            [FromQuery] int? assigneeId,
            [FromQuery] DateTime? dueBefore,
            [FromQuery] DateTime? dueAfter,
            [FromQuery] List<int> tagIds)
        {
            var hasStatus = status.HasValue;

            // ✅ Если статус НЕ задан — используем GetAllTasksAsync, который теперь поддерживает tagIds тоже
            if (!hasStatus)
            {
                var tasks = await _taskService.GetAllTasksAsync(
                    assigneeId,
                    dueBefore,
                    dueAfter,
                    (tagIds != null && tagIds.Any()) ? tagIds : null);

                return Ok(tasks);
            }

            // ✅ Если статус задан — оставляем старый путь GetFilteredTasksAsync (он уже поддерживает tagIds)
            var filtered = await _taskService.GetFilteredTasksAsync(
                status?.ToString(),
                assigneeId,
                dueBefore,
                dueAfter,
                tagIds);

            return Ok(filtered);
        }

        // Остальные методы контроллера оставляем как были в проекте
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                return Ok(TaskDto.FromEntity(task));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Внутренняя ошибка: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var task = await _taskService.CreateTaskAsync(createDto);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, TaskDto.FromEntity(task));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Внутренняя ошибка: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var task = await _taskService.UpdateTaskAsync(id, updateDto);
                return Ok(TaskDto.FromEntity(task));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Внутренняя ошибка: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}