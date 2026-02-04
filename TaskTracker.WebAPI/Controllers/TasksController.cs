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
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            return Ok(TaskDto.FromEntity(task));
        }

        // POST: api/tasks
        [HttpPost]
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

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
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

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}