using API.Models;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/environments")]
    [Authorize]
    public class EnvironmentController : ControllerBase
    {
        private readonly IEnvironment2DRepository _environmentRepo;

        public EnvironmentController(IEnvironment2DRepository environmentRepo)
        {
            _environmentRepo = environmentRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Environment2D>>> GetEnvironments()
        {
            return Ok(await _environmentRepo.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Environment2D>> GetEnvironment(Guid id)
        {
            var env = await _environmentRepo.GetByIdAsync(id);
            return env == null ? NotFound() : Ok(env);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateEnvironment(Environment2D environment)
        {
            var id = await _environmentRepo.InsertAsync(environment);
            return CreatedAtAction(nameof(GetEnvironment), new { id }, id);
        }

    }
}
