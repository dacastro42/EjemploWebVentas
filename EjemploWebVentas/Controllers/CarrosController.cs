using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EjemploWebVentas.Data;
using EjemploWebVentas.Models;

namespace EjemploWebVentas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarrosController : ControllerBase
    {
        private readonly VentasDbContext _db;

        public CarrosController(VentasDbContext db) => _db = db;

        // GET: api/Carros
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Carro>>> GetAll()
        {
            var carros = await _db.Carros
                .AsNoTracking()
                .ToListAsync();

            return Ok(carros);
        }

        // GET: api/Carros/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Carro>> GetById(int id)
        {
            var carro = await _db.Carros
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdC == id);

            if (carro == null) return NotFound();
            return Ok(carro);
        }

        // POST: api/Carros
        [HttpPost]
        public async Task<ActionResult<Carro>> Create([FromBody] Carro carro)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            _db.Carros.Add(carro);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = carro.IdC }, carro);
        }

        // PUT: api/Carros/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Carro carro)
        {
            if (id != carro.IdC) return BadRequest("El id de la URL no coincide con el id del body.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            _db.Entry(carro).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var existe = await _db.Carros.AnyAsync(c => c.IdC == id);
                if (!existe) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Carros/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var carro = await _db.Carros.FirstOrDefaultAsync(c => c.IdC == id);
            if (carro == null) return NotFound();

            _db.Carros.Remove(carro);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
