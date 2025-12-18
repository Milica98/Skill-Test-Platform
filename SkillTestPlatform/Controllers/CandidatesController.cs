using Microsoft.AspNetCore.Mvc;
using SkillTestPlatform.Models;
using SkillTestPlatform.Services.Interfaces;

namespace SkillTestPlatform.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CandidatesController(IStorageService<Candidate> storageService, ILogger<CandidatesController> logger) : ControllerBase
    {
        private readonly IStorageService<Candidate> _storageService = storageService;
        private readonly ILogger<CandidatesController> _logger = logger;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candidate>>> Get()
        {
            var candidates = await _storageService.GetAsync();
            return Ok(candidates);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Candidate>> Get(string id)
        {
            var candidate = await _storageService.GetAsync(id);
            if (candidate is null)
            {
                _logger.LogWarning("Candidate with id {Id} not found.", id);
                return NotFound();
            }
            return Ok(candidate);
        }

        [HttpPost]
        public async Task<ActionResult<Candidate>> Create([FromBody] CandidateCreateModel candidateCreateModel)
        {
            var candidate = new Candidate(candidateCreateModel);
            await _storageService.AddAsync(candidate, candidate.Id);
            return CreatedAtAction(nameof(Get), new { id = candidate.Id }, candidate);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Candidate candidate)
        {
            await _storageService.UpdateAsync(candidate, candidate.Id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _storageService.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Tried to delete candidate with id {Id}, but it was not found.", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}
