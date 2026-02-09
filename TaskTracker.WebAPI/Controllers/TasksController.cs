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
        [SwaggerOperation(Summary = "Получить задачу по ID", Description = "Возвращает задачу по указанному идентификатору")]
        [SwaggerResponse(200, "Задача найдена", typeof(TaskDto))]
        [SwaggerResponse(404, "Задача не найдена")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            return Ok(TaskDto.FromEntity(task));
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
            try
            {
                var task = await _taskService.CreateTaskAsync(createDto);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, TaskDto.FromEntity(task));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Обновить задачу
        /// </summary>
        /// <param name="id">Идентификатор задачи</param>
        /// <param name="updateDto">Данные для обновления</param>
        /// <returns>Обновлённая задача</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновить задачу", Description = "Обновляет существующую задачу")]
        [SwaggerResponse(200, "Задача обновлена", typeof(TaskDto))]
        [SwaggerResponse(400, "Неверные данные")]
        [SwaggerResponse(404, "Задача не найдена")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateDto)
        {
            try
            {
                var task = await _taskService.UpdateTaskAsync(id, updateDto);
                if (task == null)
                    return NotFound();

                return Ok(TaskDto.FromEntity(task));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
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
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Получить задачи с фильтрацией
        /// </summary>
        /// <param name="status">Фильтр по статусу</param>
        /// <param name="assigneeId">Фильтр по исполнителю</param>
        /// <param name="dueBefore">Дедлайн до даты</param>
        /// <param name="dueAfter">Дедлайн после даты</param>
        /// <param name="tagId">Фильтр по тегу</param>
        /// <returns>Отфильтрованный список задач</returns>
        [HttpGet("filter")]
        [SwaggerOperation(Summary = "Фильтрация задач", Description = "Возвращает задачи с применением фильтров")]
        [SwaggerResponse(200, "Успешный запрос", typeof(List<TaskDto>))]
        public async Task<IActionResult> GetFilteredTasks(
            [FromQuery] string status = default,
            [FromQuery] int? assigneeId = null,
            [FromQuery] DateTime? dueBefore = null,
            [FromQuery] DateTime? dueAfter = null,
            [FromQuery] List<int> tagId = default)
        {
            var tasks = await _taskService.GetFilteredTasksAsync(status, assigneeId, dueBefore, dueAfter, tagId);
            return Ok(tasks);
        }
    }
}