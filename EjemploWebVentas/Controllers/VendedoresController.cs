using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EjemploWebVentas.Data;
using EjemploWebVentas.Models;

namespace EjemploWebVentas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendedoresController : ControllerBase
    {
        private readonly VentasDbContext _db;

        public VendedoresController(VentasDbContext db) => _db = db;

        // GET: api/vendedores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vendedor>>> GetAll()
        {
            var vendedores = await _db.Vendedores
                .AsNoTracking()
                .ToListAsync();

            return Ok(vendedores);
        }

        // GET: api/vendedores/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Vendedor>> GetById(int id)
        {
            var vendedor = await _db.Vendedores
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdV == id);

            if (vendedor == null) return NotFound();
            return Ok(vendedor);
        }

        // POST: api/vendedores
        [HttpPost]
        public async Task<ActionResult<Vendedor>> Create([FromBody] Vendedor vendedor)
        {
            // Nota didáctica: validar ModelState en API
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            _db.Vendedores.Add(vendedor);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vendedor.IdV }, vendedor);
        }

        // PUT: api/vendedores/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Vendedor vendedor)
        {
            if (id != vendedor.IdV) return BadRequest("El id de la URL no coincide con el id del body.");

            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Marcar entidad como modificada
            _db.Entry(vendedor).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var existe = await _db.Vendedores.AnyAsync(v => v.IdV == id);
                if (!existe) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/vendedores/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdV == id);
            if (vendedor == null) return NotFound();

            _db.Vendedores.Remove(vendedor);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
