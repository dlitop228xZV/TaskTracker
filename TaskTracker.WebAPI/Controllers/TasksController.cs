using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

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

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTasks(
            [FromQuery] string? status,
            [FromQuery] int? assigneeId,
            [FromQuery] DateTime? dueBefore,
            [FromQuery] DateTime? dueAfter,
            [FromQuery] List<int>? tagIds)
        {
            var tasks = await _taskService.GetAllTasksAsync(status, assigneeId, dueBefore, dueAfter, tagIds);

            var result = tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.AssigneeId,
                AssigneeName = t.Assignee?.Name,
                t.CreatedAt,
                t.DueDate,
                t.CompletedAt,
                t.Status,
                t.Priority,
                Tags = t.TaskTags?.Select(tt => tt.Tag?.Name).Where(name => name != null)
            });

            return Ok(result);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound(new { message = $"Task with id {id} not found" });

            return Ok(task);
        }

        // POST: api/tasks
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateTask([FromBody] CreateTaskDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var task = await _taskService.CreateTaskAsync(createDto);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
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

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedTask = await _taskService.UpdateTaskAsync(id, updateDto);
                return Ok(updatedTask);
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
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Удалить задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <returns>Результат удаления</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удалить задачу", Description = "Удаляет задачу по идентификатору")]
        [SwaggerResponse(204, "Задача удалена")]
        [SwaggerResponse(404, "Задача не найдена")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result)
                return NotFound(new { error = $"Задача с ID {id} не найдена" });

            return NoContent();
        }

        /// <summary>
        /// Получить задачи с фильтрацией
        /// </summary>
        /// <param name="status">Фильтр по статусу</param>
        /// <param name="assigneeId">Фильтр по исполнителю</param>
        /// <param name="dueBefore">Дедлайн до даты</param>
        /// <param name="dueAfter">Дедлайн после даты</param>
        /// <param name="tagIds">Фильтр по тегам</param>
        /// <returns>Отфильтрованный список задач</returns>
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

        /// <summary>
        /// Изменить статус задачи
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="newStatus">Новый статус (New, InProgress, Done)</param>
        /// <returns>Результат операции</returns>
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

        /// <summary>
        /// Получить просроченные задачи
        /// </summary>
        /// <returns>Список просроченных задач</returns>
        [HttpGet("overdue")]
        [SwaggerOperation(Summary = "Получить просроченные задачи", Description = "Возвращает список задач, у которых истёк срок выполнения")]
        [SwaggerResponse(200, "Успешный запрос", typeof(List<TaskDto>))]
        public async Task<IActionResult> GetOverdueTasks()
        {
            var tasks = await _taskService.GetOverdueTasksAsync();
            return Ok(tasks);
        }

        /// <summary>
        /// Получить количество задач по статусу
        /// </summary>
        /// <param name="status">Статус для фильтрации</param>
        /// <returns>Количество задач</returns>
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