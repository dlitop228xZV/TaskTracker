using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.WebAPI.Controllers
{
    /// <summary>
    /// Контроллер для управления задачами
    /// </summary>
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
        /// Получить список всех задач
        /// </summary>
        /// <returns>Список задач</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получить все задачи", Description = "Возвращает список всех задач в системе")]
        [SwaggerResponse(200, "Успешный запрос", typeof(List<TaskDto>))]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        /// <summary>
        /// Получить задачу по ID
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <returns>Задача</returns>
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

        /// <summary>
        /// Создать новую задачу
        /// </summary>
        /// <param name="createDto">Данные для создания задачи</param>
        /// <returns>Созданная задача</returns>
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

        /// <summary>
        /// Обновить задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="updateDto">Данные для обновления</param>
        /// <returns>Обновлённая задача</returns>
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
