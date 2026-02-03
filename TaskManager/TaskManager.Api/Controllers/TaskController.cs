using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var tasks = await _taskService.GetAllAsync(cancellationToken);
                return Ok(tasks);
            }
            catch (Exception)
            {
                return StatusCode(500, "Server Error.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var task = await _taskService.GetByIdAsync(id, cancellationToken);
                if (task == null)
                {
                    return NotFound();
                }
                return Ok(task);
            }
            catch (Exception)
            {
                return StatusCode(500, "Server Error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var createdTask = await _taskService.CreateAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
            }
            catch (Exception)
            {
                return StatusCode(500, "Server Error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var updatedTask = await _taskService.UpdateAsync(id, dto, cancellationToken);
                if (updatedTask == null)
                {
                    return NotFound();
                }
                return Ok(updatedTask);
            }
            catch (Exception)
            {
                return StatusCode(500, "Server Error.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _taskService.DeleteAsync(id, cancellationToken);
                if (!deleted)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Server Error.");
            }
        }

        [HttpPost("test")]
        public IActionResult CreateTest()
        {
            return Ok(new { Message = "Test endpoint works!" });
        }
    }
}