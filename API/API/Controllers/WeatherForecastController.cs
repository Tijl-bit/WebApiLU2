using API.Models;
using API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/objects")]
    public class ObjectController : ControllerBase
    {
        private readonly IObject2DRepository _objectRepo;

        public ObjectController(IObject2DRepository objectRepo)
        {
            _objectRepo = objectRepo;
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
        public async Task<ActionResult<Guid>> CreateObject(Object2D object2D)
        {
            var id = await _objectRepo.InsertAsync(object2D);
            return CreatedAtAction(nameof(GetObject), new { id }, id);
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
