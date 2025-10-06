using GraduationProject.DTOs;
using GraduationProject.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _svc;
        public ProductsController(IProductService svc) => _svc = svc;

        // GET /api/products
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CProductDTO>>> List(CancellationToken ct)
            => Ok(await _svc.ListAsync(ct));

        // GET /api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CProductDTO>> Get(int id, CancellationToken ct)
        {
            var dto = await _svc.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // POST /api/products
        [HttpPost]
        public async Task<ActionResult<CProductDTO>> Create([FromBody] CProductCreateDTO input, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _svc.CreateAsync(input, ct);
            return CreatedAtAction(nameof(Get), new { id = created.ProductId }, created);
        }

        // PUT /api/products/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CProductDTO>> Update(int id, [FromBody] CProductUpdateDTO input, CancellationToken ct)
        {
            if (id != input.ProductId) return BadRequest("Route id and body id mismatch.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var updated = await _svc.UpdateAsync(input, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE /api/products/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await _svc.DeleteAsync(id, ct);
            return ok ? Ok(new { Result = "OK" }) : NotFound(new { Result = "Error", Message = "Record not found." });
        }
    }
}
