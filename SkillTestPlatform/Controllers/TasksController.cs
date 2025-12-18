using Microsoft.AspNetCore.Mvc;
using SkillTestPlatform.Models;
using SkillTestPlatform.Services.Interfaces;

namespace SkillTestPlatform.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TasksController(IStorageService<TaskItem> storageService, ILogger<TasksController> logger) : ControllerBase
    {
        private readonly IStorageService<TaskItem> _storageService = storageService;
        private readonly ILogger<TasksController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> Get()
        {
            var tasks = await _storageService.GetAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> Get(string id)
        {
            var task = await _storageService.GetAsync(id);
            if (task is null)
            {
                _logger.LogWarning("Task with id {Id} not found.", id);
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create([FromBody] TaskItemCreateModel taskItemCreateModel)
        {
            var task = new TaskItem(taskItemCreateModel);
            await _storageService.AddAsync(task, task.Id);
            return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TaskItem task)
        {
            await _storageService.UpdateAsync(task, task.Id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _storageService.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Tried to delete task with id {Id}, but it was not found.", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}
