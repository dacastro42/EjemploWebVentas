using EjemploWebVentas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EjemploWebVentas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly VentasDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(VentasDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public record LoginDto(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var vendedor = await _db.Vendedores
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.EmailV == dto.Email);

        if (vendedor == null) return Unauthorized("Credenciales inválidas.");

        // ✅ Para clase inicial: password en texto plano
        if (vendedor.PasswordV != dto.Password)
            return Unauthorized("Credenciales inválidas.");

        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, vendedor.IdV.ToString()),
            new Claim(ClaimTypes.Email, vendedor.EmailV),
            new Claim(ClaimTypes.Name, $"{vendedor.Nombre1V} {vendedor.Apellido1V}")
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!)),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
