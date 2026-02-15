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
        /// Примеры запросов:
        /// GET /api/tasks
        /// GET /api/tasks?assigneeId=2
        /// GET /api/tasks?dueBefore=2026-02-20
        /// GET /api/tasks?dueAfter=2026-02-01&amp;dueBefore=2026-02-20
        /// GET /api/tasks?status=InProgress&amp;assigneeId=2&amp;dueBefore=2026-02-20
        ///
        /// Пример фрагмента ответа (важное поле EffectiveStatus):
        /// [
        ///   {
        ///     "id": 1,
        ///     "title": "Fix bug",
        ///     "status": "InProgress",
        ///     "effectiveStatus": "Overdue"
        ///   }
        /// ]
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
            var hasTags = tagIds != null && tagIds.Any();

            List<TaskDto> tasks;

            if (!hasStatus && !hasTags)
            {
                tasks = await _taskService.GetAllTasksAsync(assigneeId, dueBefore, dueAfter);
            }
            else
            {
                tasks = await _taskService.GetFilteredTasksAsync(
                    status?.ToString(),
                    assigneeId,
                    dueBefore,
                    dueAfter,
                    tagIds);
            }

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получить задачу по ID", Description = "Возвращает детальную информацию о задаче")]
        [SwaggerResponse(200, "Задача найдена", typeof(TaskDto))]
        [SwaggerResponse(404, "Задача не найдена")]
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
        [SwaggerOperation(Summary = "Создать задачу", Description = "Создаёт новую задачу с указанными параметрами")]
        [SwaggerResponse(201, "Задача создана", typeof(TaskDto))]
        [SwaggerResponse(400, "Неверные данные")]
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
        [SwaggerOperation(Summary = "Обновить задачу", Description = "Обновляет существующую задачу. При переводе в статус Done автоматически устанавливает дату завершения.")]
        [SwaggerResponse(200, "Задача обновлена", typeof(TaskDto))]
        [SwaggerResponse(400, "Неверные данные")]
        [SwaggerResponse(404, "Задача не найдена")]
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
        [SwaggerOperation(Summary = "Удалить задачу", Description = "Удаляет задачу по идентификатору")]
        [SwaggerResponse(204, "Задача удалена")]
        [SwaggerResponse(404, "Задача не найдена")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("filter")]
        [SwaggerOperation(Summary = "Фильтрация задач", Description = "Возвращает задачи с применением фильтров")]
        [SwaggerResponse(200, "Успешный запрос", typeof(List<TaskDto>))]
        public async Task<IActionResult> GetFilteredTasks(
            [FromQuery] string status = default,
            [FromQuery] int? assigneeId = null,
            [FromQuery] DateTime? dueBefore = null,
            [FromQuery] DateTime? dueAfter = null,
            [FromQuery] List<int> tagIds = default)
        {
            try
            {
                var tasks = await _taskService.GetFilteredTasksAsync(status, assigneeId, dueBefore, dueAfter, tagIds);
                return Ok(tasks);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [SwaggerOperation(Summary = "Изменить статус задачи", Description = "Изменяет статус задачи. При переводе в Done автоматически устанавливается дата завершения.")]
        [SwaggerResponse(200, "Статус изменён")]
        [SwaggerResponse(400, "Неверный статус")]
        [SwaggerResponse(404, "Задача не найдена")]
        public async Task<IActionResult> ChangeTaskStatus(int id, [FromQuery] string newStatus)
        {
            try
            {
                var result = await _taskService.ChangeTaskStatusAsync(id, newStatus);
                if (!result)
                    return BadRequest(new { error = $"Некорректный статус: {newStatus}" });

                return Ok(new { message = "Статус задачи успешно изменён" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("overdue")]
        [SwaggerOperation(Summary = "Получить просроченные задачи", Description = "Возвращает список задач, у которых истёк срок выполнения")]
        [SwaggerResponse(200, "Успешный запрос", typeof(List<TaskDto>))]
        public async Task<IActionResult> GetOverdueTasks()
        {
            var tasks = await _taskService.GetOverdueTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("count-by-status")]
        [SwaggerOperation(Summary = "Количество задач по статусу", Description = "Возвращает количество задач с указанным статусом")]
        [SwaggerResponse(200, "Успешный запрос")]
        [SwaggerResponse(400, "Неверный статус")]
        public async Task<IActionResult> GetTasksCountByStatus([FromQuery] string status)
        {
            try
            {
                var count = await _taskService.GetTasksCountByStatusAsync(status);
                return Ok(new { status, count });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
