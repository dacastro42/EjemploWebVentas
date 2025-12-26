using EjemploWebVentas.Data;
using EjemploWebVentas.DTOs;
using EjemploWebVentas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EjemploWebVentas.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly VentasDbContext _db;
        const decimal IVA_RATE = 0.19m;

        public VentasController(VentasDbContext db) => _db = db;

        // GET: api/Ventas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ventas = await _db.Ventas
                .AsNoTracking()
                .Include(v => v.Vendedor)
                .OrderByDescending(v => v.FechaVenta)
                .Select(v => new
                {
                    v.IsVentas,          // <-- antes IdVentas
                    v.FechaVenta,
                    v.Iva,
                    v.TotalVenta,
                    Vendedor = v.Vendedor == null ? null : new
                    {
                        v.Vendedor.IdV,
                        v.Vendedor.Nombre1V,
                        v.Vendedor.Apellido1V,
                        v.Vendedor.EmailV
                    }
                })
                .ToListAsync();

            return Ok(ventas);
        }

        // GET: api/Ventas/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var venta = await _db.Ventas
                .AsNoTracking()
                .Include(v => v.Vendedor)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Carro)
                .FirstOrDefaultAsync(v => v.IsVentas == id);

            if (venta == null) return NotFound();

            var response = new
            {
                venta.IsVentas,
                venta.FechaVenta,
                venta.Iva,
                venta.TotalVenta,
                Vendedor = venta.Vendedor == null ? null : new
                {
                    venta.Vendedor.IdV,
                    venta.Vendedor.Nombre1V,
                    venta.Vendedor.Nombre2V,
                    venta.Vendedor.Apellido1V,
                    venta.Vendedor.Apellido2V,
                    venta.Vendedor.EmailV,
                    venta.Vendedor.TelefonoV
                },
                Detalles = venta.Detalles.Select(d => new
                {
                    d.IdVentaDetalles,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Subtotal,
                    Carro = d.Carro == null ? null : new
                    {
                        d.Carro.IdC,
                        d.Carro.Marca,
                        d.Carro.Modelo,
                        d.Carro.Anio,
                        d.Carro.PrecioC
                    }
                })
            };

            return Ok(response);
        }

        // POST: api/Ventas
        //falta agregar impuestos 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VentaCreateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest("La venta debe tener al menos un detalle.");

            // 1) Validar vendedor existe
            var vendedorExiste = await _db.Vendedores.AnyAsync(v => v.IdV == dto.VendedorId);
            if (!vendedorExiste) return BadRequest($"No existe vendedor con id {dto.VendedorId}.");

            // 2) Validar carros y obtener precios
            var carroIds = dto.Detalles.Select(d => d.CarroId).Distinct().ToList();
            var carros = await _db.Carros
                .AsNoTracking()
                .Where(c => carroIds.Contains(c.IdC))
                .Select(c => new { c.IdC, c.PrecioC })
                .ToListAsync();

            if (carros.Count != carroIds.Count)
            {
                var encontrados = carros.Select(c => c.IdC).ToHashSet();
                var faltantes = carroIds.Where(id => !encontrados.Contains(id)).ToList();
                return BadRequest(new { message = "Hay carros inexistentes.", faltantes });
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                var venta = new Venta
                {
                    VendedorId = dto.VendedorId,
                    FechaVenta = DateTime.UtcNow,
                    TotalVenta = 0m
                };

                _db.Ventas.Add(venta);
                await _db.SaveChangesAsync(); // aquí se genera venta.IsVentas (AUTO_INCREMENT)

                decimal subtotalVenta = 0m;

                foreach (var det in dto.Detalles)
                {
                    var precio = carros.First(c => c.IdC == det.CarroId).PrecioC;
                    var subtotal = det.Cantidad * precio;

                    var detalle = new VentaDetalle
                    {
                        VentaId = venta.IsVentas,
                        CarroId = det.CarroId,
                        Cantidad = det.Cantidad,
                        PrecioUnitario = precio,
                        Subtotal = subtotal
                    };

                    subtotalVenta += subtotal;
                    _db.VentaDetalles.Add(detalle);
                }

                // IVA en dinero y total final
                venta.Iva = Math.Round(subtotalVenta * IVA_RATE, 2);
                venta.TotalVenta = subtotalVenta + venta.Iva;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = venta.IsVentas }, new
                {
                    venta.IsVentas,
                    venta.VendedorId,
                    venta.FechaVenta,
                    iva = venta.Iva,
                    totalVenta = venta.TotalVenta
                });
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                return Problem(
                    title: "Error guardando la venta en la base de datos",
                    detail: ex.InnerException?.Message ?? ex.Message,
                    statusCode: 500
                );
            }
        }

        #region Consultas
        // GET: api/Ventas/reportes/mayor-venta
        [HttpGet("reportes/mayor-venta")]
        public async Task<IActionResult> MayorVenta()
        {
            var mayor = await (
                from v in _db.Ventas.AsNoTracking()
                join ven in _db.Vendedores.AsNoTracking()
                    on v.VendedorId equals ven.IdV
                orderby v.TotalVenta descending
                select new
                {
                    v.IsVentas,
                    v.FechaVenta,
                    v.TotalVenta,
                    v.VendedorId,
                    Vendedor = new
                    {
                        ven.IdV,
                        ven.Nombre1V,
                        ven.Apellido1V,
                        ven.EmailV
                    }
                }
            ).FirstOrDefaultAsync();

            if (mayor == null) return NotFound("No hay ventas registradas.");
            return Ok(mayor);
        }


        // GET: api/Ventas/reportes/vendedor-top?anio=2025&mes=12
        [HttpGet("reportes/vendedor-top")]
        public async Task<IActionResult> VendedorTop([FromQuery] int? anio, [FromQuery] int? mes)
        {
            var q = _db.Ventas.AsNoTracking();

            if (anio.HasValue)
                q = q.Where(v => v.FechaVenta.Year == anio.Value);

            if (mes.HasValue)
                q = q.Where(v => v.FechaVenta.Month == mes.Value);

            var top = await q
                .GroupBy(v => v.VendedorId)
                .Select(g => new
                {
                    VendedorId = g.Key,
                    TotalVendido = g.Sum(x => x.TotalVenta),
                    CantidadVentas = g.Count()
                })
                .OrderByDescending(x => x.TotalVendido)
                .FirstOrDefaultAsync();

            if (top == null) return NotFound("No hay ventas para el filtro dado.");

            var vendedor = await _db.Vendedores
                .AsNoTracking()
                .Where(v => v.IdV == top.VendedorId)
                .Select(v => new { v.IdV, v.Nombre1V, v.Apellido1V, v.EmailV })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                Vendedor = vendedor,
                top.TotalVendido,
                top.CantidadVentas
            });
        }


        // GET: api/Ventas/reportes/total-mes?anio=2025&mes=12
        [HttpGet("reportes/total-mes")]
        public async Task<IActionResult> TotalMes([FromQuery] int anio, [FromQuery] int mes)
        {
            if (mes < 1 || mes > 12) return BadRequest("El mes debe estar entre 1 y 12.");

            var total = await _db.Ventas
                .AsNoTracking()
                .Where(v => v.FechaVenta.Year == anio && v.FechaVenta.Month == mes)
                .SumAsync(v => (decimal?)v.TotalVenta) ?? 0m;

            var cantidad = await _db.Ventas
                .AsNoTracking()
                .Where(v => v.FechaVenta.Year == anio && v.FechaVenta.Month == mes)
                .CountAsync();

            return Ok(new
            {
                anio,
                mes,
                cantidadVentas = cantidad,
                totalVendido = total
            });
        }

        // GET: api/Ventas/reportes/carro-mas-vendido?anio=2025&mes=12
        [HttpGet("reportes/carro-mas-vendido")]
        public async Task<IActionResult> CarroMasVendido([FromQuery] int? anio, [FromQuery] int? mes)
        {
            // JOIN: detalles -> ventas (para filtrar por fecha)
            var baseQuery =
                from d in _db.VentaDetalles.AsNoTracking()
                join v in _db.Ventas.AsNoTracking()
                    on d.VentaId equals v.IsVentas
                select new { d, v };

            if (anio.HasValue)
                baseQuery = baseQuery.Where(x => x.v.FechaVenta.Year == anio.Value);

            if (mes.HasValue)
                baseQuery = baseQuery.Where(x => x.v.FechaVenta.Month == mes.Value);

            var top = await baseQuery
                .GroupBy(x => x.d.CarroId)
                .Select(g => new
                {
                    CarroId = g.Key,
                    Unidades = g.Sum(x => x.d.Cantidad),
                    TotalFacturado = g.Sum(x => x.d.Subtotal)
                })
                .OrderByDescending(x => x.Unidades)
                .FirstOrDefaultAsync();

            if (top == null) return NotFound("No hay detalles de venta para el filtro dado.");

            var carro = await _db.Carros
                .AsNoTracking()
                .Where(c => c.IdC == top.CarroId)
                .Select(c => new { c.IdC, c.Marca, c.Modelo, c.Anio, c.PrecioC })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                Carro = carro,
                top.Unidades,
                top.TotalFacturado
            });
        }
        #endregion
    }
}
