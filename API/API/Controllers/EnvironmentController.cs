using System;
using API.Models;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/environments")]
    public class EnvironmentController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEnvironment2DRepository _environment2DRepository;

        public EnvironmentController(IAuthenticationService authenticationService, IEnvironment2DRepository environment2DRepository)
        {
            _authenticationService = authenticationService;
            _environment2DRepository = environment2DRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Environment2D>>> GetEnvironments()
        {
            var id = _authenticationService.GetCurrentAuthenticatedUserId();
            return Ok(await _environment2DRepository.GetAllAsync(id));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Environment2D>> GetEnvironment(Guid id)
        {
            var env = await _environment2DRepository.GetByIdAsync(id);
            return env == null ? NotFound() : Ok(env);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnvironment(Guid id)
        {
            var env = await _environment2DRepository.GetByIdAsync(id);
            if (env == null)
                return NotFound();

            // Optional: Only allow the owner to delete
            var currentUserId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (env.OwnerUserId != currentUserId)
                return Unauthorized("You are not allowed to delete this environment.");

            await _environment2DRepository.DeleteAsync(id);
            return NoContent(); // 204 No Content is standard for successful deletes
        }



        [HttpPost]
        public async Task<ActionResult<Guid>> CreateEnvironment(Environment2D environment)
        {


            if (environment == null)
                return BadRequest("Invalid environment object");

            // Get current user ID
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();

            // Ensure that the user is authenticated and has a valid userId
            if (userId == null)
                return Unauthorized("User is not authenticated");

            environment.OwnerUserId = userId;

            // Get all environments and count how many this user owns

            var existingEnvironments = await _environment2DRepository.GetAllAsync(userId);
            var userEnvironmentCount = existingEnvironments.Count(e => e.OwnerUserId == userId);

            // Check max limit
            if (userEnvironmentCount >= 5)
                return BadRequest("You can only have a maximum of 5 worlds.");

            // Set the ID if it's not set
            if (environment.Id == Guid.Empty)
            {
                environment.Id = Guid.NewGuid();
            }

            var id = await _environment2DRepository.InsertAsync(environment);
            return CreatedAtAction(nameof(GetEnvironment), new { id = id }, id);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEnvironment2D(Environment2D environment)
        {
            if (environment == null)
                return BadRequest("Invalid environment object");

            if (environment.Id == Guid.Empty)
                return BadRequest("Invalid ID");

            if (environment.OwnerUserId != _authenticationService.GetCurrentAuthenticatedUserId())
                return Unauthorized("User is not allowed to view the environment");

            return Ok("Environment2D object updated");
        }
    }
}