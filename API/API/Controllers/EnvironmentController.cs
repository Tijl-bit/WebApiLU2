using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
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

        // GET all environments for the authenticated user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Environment2D>>> GetEnvironments()
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            return Ok(await _environment2DRepository.GetAllAsync(userId));
        }

        // GET a specific environment by ID (only if owned by current user)
        [HttpGet("{id}")]
        public async Task<ActionResult<Environment2D>> GetEnvironment(Guid id)
        {
            var env = await _environment2DRepository.GetByIdAsync(id);
            var currentUserId = _authenticationService.GetCurrentAuthenticatedUserId();

            if (env == null)
                return NotFound();

            if (env.OwnerUserId != currentUserId)
                return Unauthorized("You are not allowed to access this environment.");

            return Ok(env);
        }

        // DELETE environment (only if owned by current user)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnvironment(Guid id)
        {
            var env = await _environment2DRepository.GetByIdAsync(id);

            if (env == null)
                return NotFound();

            var currentUserId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (env.OwnerUserId != currentUserId)
                return Unauthorized("You are not allowed to delete this environment.");

            await _environment2DRepository.DeleteAsync(id);
            return NoContent();
        }

        // POST new environment (limit to 5 per user)
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateEnvironment(PostEnvironment2D environment)
        {
            if (environment == null)
                return BadRequest("Invalid environment object");

            var userId = _authenticationService.GetCurrentAuthenticatedUserId();

            // ✅ Validate Name
            if (string.IsNullOrWhiteSpace(environment.Name))
                return BadRequest("Environment name cannot be empty.");

            if (environment.Name.Length > 50) // adjust max length as needed
                return BadRequest("Environment name cannot be longer than 50 characters.");

            var existingEnvironments = await _environment2DRepository.GetAllAsync(userId);
            var userEnvironmentCount = existingEnvironments.Count();

            if (userEnvironmentCount >= 5)
                return BadRequest("You can only have a maximum of 5 worlds.");

            var environ = new Environment2D
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                Name = environment.Name,
                MaxLength = environment.MaxLength,
                MaxHeight = environment.MaxHeight
            };

            var id = await _environment2DRepository.InsertAsync(environ);
            return CreatedAtAction(nameof(GetEnvironment), new { id }, id);
        }


        // PUT update environment (only if owned by current user)
        [HttpPut]
        public async Task<IActionResult> UpdateEnvironment2D(Environment2D environment)
        {
            if (environment == null || environment.Id == Guid.Empty)
                return BadRequest("Invalid environment object");

            var currentUserId = _authenticationService.GetCurrentAuthenticatedUserId();

            var existingEnv = await _environment2DRepository.GetByIdAsync(environment.Id);
            if (existingEnv == null)
                return NotFound();

            if (existingEnv.OwnerUserId != currentUserId)
                return Unauthorized("You are not allowed to update this environment.");

            // Only allow update of Name, MaxHeight, MaxLength
            existingEnv.Name = environment.Name;
            existingEnv.MaxHeight = environment.MaxHeight;
            existingEnv.MaxLength = environment.MaxLength;

            await _environment2DRepository.UpdateAsync(existingEnv);
            return Ok("Environment updated successfully.");
        }
    }
}
