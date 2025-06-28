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
          
            var userId = _authService.GetCurrentAuthenticatedUserId();
            var environments = await _envRepo.GetAllAsync(userId);
            var envIds = environments.Select(e => e.Id).ToHashSet();
            var allObjects = await _objectRepo.GetAllAsync();
            var userObjects = allObjects.Where(o => envIds.Contains(o.EnvironmentId));
            return Ok(userObjects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Object2D>> GetObject(Guid id)
        {
            var obj = await _objectRepo.GetByIdAsync(id);
            if (obj == null)
                return NotFound();

            var env = await _envRepo.GetByIdAsync(obj.EnvironmentId);
            if (env == null || env.OwnerUserId != _authService.GetCurrentAuthenticatedUserId())
                return Unauthorized("You do not own this environment");

            return Ok(obj);
        }

        [HttpPost]
        public async Task<IActionResult> AddObject([FromBody] PostObject2D object2D)
        {
            if (object2D == null)
                return BadRequest("Invalid object data");

            var environmentId = Guid.Parse(object2D.EnvironmentId);
            var environment = await _envRepo.GetByIdAsync(environmentId);
            if (environment == null)
                return NotFound("Environment not found");

            if (environment.OwnerUserId != _authService.GetCurrentAuthenticatedUserId())
                return Unauthorized("You do not own this environment");

            Object2D objecten2D = new Object2D
            {
                Id = Guid.NewGuid(),
                EnvironmentId = environmentId,
                PrefabId = object2D.PrefabId,
                PositionX = object2D.PositionX,
                PositionY = object2D.PositionY,
                RotationZ = object2D.RotationZ,
                ScaleX = object2D.ScaleX,
                ScaleY = object2D.ScaleY,
                SortingLayer = object2D.SortingLayer
            };

            await _objectRepo.InsertAsync(objecten2D);

            return Ok(object2D);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateObject(Guid id, Object2D object2D)
        {
            if (id != object2D.Id)
                return BadRequest("Object ID mismatch");

            var env = await _envRepo.GetByIdAsync(object2D.EnvironmentId);
            if (env == null || env.OwnerUserId != _authService.GetCurrentAuthenticatedUserId())
                return Unauthorized("You do not own this environment");

            return await _objectRepo.UpdateAsync(object2D) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObject(Guid id)
        {
            var obj = await _objectRepo.GetByIdAsync(id);
            if (obj == null)
                return NotFound();

            var env = await _envRepo.GetByIdAsync(obj.EnvironmentId);
            if (env == null || env.OwnerUserId != _authService.GetCurrentAuthenticatedUserId())
                return Unauthorized("You do not own this environment");

            return await _objectRepo.DeleteAsync(id) ? NoContent() : NotFound();
        }

        [HttpGet("by-environment")]
        public async Task<IActionResult> GetByEnvironment([FromQuery] Guid environmentId)
        {
            var env = await _envRepo.GetByIdAsync(environmentId);
            if (env == null)
                return NotFound("Environment not found");

            if (env.OwnerUserId != _authService.GetCurrentAuthenticatedUserId())
                return Unauthorized("You do not own this environment");

            var objects = await _objectRepo.GetByEnvironmentIdAsync(environmentId);
            return Ok(objects);
        }
    }
}
