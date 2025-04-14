using System;
using API.Models;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers
{
    [ApiController]
    [Route("api/objects")]
    [Authorize]
    public class ObjectController : ControllerBase
    {
        private readonly IObject2DRepository _objectRepo;
        private readonly IEnvironment2DRepository _envRepo;
        private readonly IAuthenticationService _authService;

        public ObjectController(
            IObject2DRepository objectRepo,
            IEnvironment2DRepository envRepo,
            IAuthenticationService authService)
        {
            _objectRepo = objectRepo;
            _envRepo = envRepo;
            _authService = authService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object2D>>> GetObjects()
        {
            return Ok(await _objectRepo.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Object2D>> GetObject(Guid id)
        {
            var obj = await _objectRepo.GetByIdAsync(id);
            return obj == null ? NotFound() : Ok(obj);
        }

        [HttpPost]
        public async Task<IActionResult> AddObject([FromBody] Object2D object2D)
        {
            if (object2D.EnvironmentId == Guid.Empty)
                return BadRequest("Invalid environment ID");

            var environment = await _envRepo.GetByIdAsync(object2D.EnvironmentId);
            if (environment == null)
                return NotFound("Environment not found");

            if (environment.OwnerUserId != _authService.GetCurrentAuthenticatedUserId())
                return Unauthorized("You do not own this environment");

            object2D.Id = Guid.NewGuid();
            await _objectRepo.InsertAsync(object2D);

            return Ok(object2D);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateObject(Guid id, Object2D object2D)
        {
            if (id != object2D.Id) return BadRequest();
            return await _objectRepo.UpdateAsync(object2D) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObject(Guid id)
        {
            return await _objectRepo.DeleteAsync(id) ? NoContent() : NotFound();
        }
    }
}
