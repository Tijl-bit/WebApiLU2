using API.Models;
using API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnvironmentObjectController : ControllerBase
    {
        private readonly IEnvironment2DRepository _environmentRepo;
        private readonly IObject2DRepository _objectRepo;

        public EnvironmentObjectController(IEnvironment2DRepository environmentRepo, IObject2DRepository objectRepo)
        {
            _environmentRepo = environmentRepo;
            _objectRepo = objectRepo;
        }

        // Environment2D Endpoints
        [HttpGet("environments")]
        public async Task<ActionResult<IEnumerable<Environment2D>>> GetEnvironments()
        {
            return Ok(await _environmentRepo.GetAllAsync());
        }

        [HttpGet("environments/{id}")]
        public async Task<ActionResult<Environment2D>> GetEnvironment(Guid id)
        {
            var env = await _environmentRepo.GetByIdAsync(id);
            return env == null ? NotFound() : Ok(env);
        }

        [HttpPost("environments")]
        public async Task<ActionResult<Guid>> CreateEnvironment(Environment2D environment)
        {
            var id = await _environmentRepo.InsertAsync(environment);
            return CreatedAtAction(nameof(GetEnvironment), new { id }, id);
        }

        [HttpPut("environments/{id}")]
        public async Task<IActionResult> UpdateEnvironment(Guid id, Environment2D environment)
        {
            if (id != environment.Id) return BadRequest();
            return await _environmentRepo.UpdateAsync(environment) ? NoContent() : NotFound();
        }

        [HttpDelete("environments/{id}")]
        public async Task<IActionResult> DeleteEnvironment(Guid id)
        {
            return await _environmentRepo.DeleteAsync(id) ? NoContent() : NotFound();
        }

        // Object2D Endpoints
        [HttpGet("objects")]
        public async Task<ActionResult<IEnumerable<Object2D>>> GetObjects()
        {
            return Ok(await _objectRepo.GetAllAsync());
        }

        [HttpGet("objects/{id}")]
        public async Task<ActionResult<Object2D>> GetObject(Guid id)
        {
            var obj = await _objectRepo.GetByIdAsync(id);
            return obj == null ? NotFound() : Ok(obj);
        }

        [HttpPost("objects")]
        public async Task<ActionResult<Guid>> CreateObject(Object2D object2D)
        {
            var id = await _objectRepo.InsertAsync(object2D);
            return CreatedAtAction(nameof(GetObject), new { id }, id);
        }

        [HttpPut("objects/{id}")]
        public async Task<IActionResult> UpdateObject(Guid id, Object2D object2D)
        {
            if (id != object2D.Id) return BadRequest();
            return await _objectRepo.UpdateAsync(object2D) ? NoContent() : NotFound();
        }

        [HttpDelete("objects/{id}")]
        public async Task<IActionResult> DeleteObject(Guid id)
        {
            return await _objectRepo.DeleteAsync(id) ? NoContent() : NotFound();
        }
    }
}
