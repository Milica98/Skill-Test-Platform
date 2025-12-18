using Microsoft.AspNetCore.Mvc;
using SkillTestPlatform.Models;
using SkillTestPlatform.Services.Interfaces;

namespace SkillTestPlatform.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SkillsController(IStorageService<Skill> storageService, ILogger<SkillsController> logger) : ControllerBase
    {
        private readonly IStorageService<Skill> _storageService = storageService;
        private readonly ILogger<SkillsController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Skill>>> Get()
        {
            var skills = await _storageService.GetAsync();
            return Ok(skills);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Skill>> Get(string id)
        {
            var skill = await _storageService.GetAsync(id);
            if (skill is null)
            {
                _logger.LogWarning("Skill with ID {SkillId} not found.", id);
                return NotFound();
            }
            return Ok(skill);
        }

        [HttpPost]
        public async Task<ActionResult<Skill>> Add([FromBody] SkillCreateModel skillCreateModel)
        {
            var skill = new Skill(skillCreateModel);
            await _storageService.AddAsync(skill, skill.Id);
            return CreatedAtAction(nameof(Get), new { id = skill.Id }, skill);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Skill skill)
        {
            await _storageService.UpdateAsync(skill, skill.Id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _storageService.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Tried to delete skill with id {SkillId}, but it was not found.", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}
