using EjemploWebVentas.Data;
using Microsoft.EntityFrameworkCore;

// https://localhost:7201/swagger/index.html

var builder = WebApplication.CreateBuilder(args);

// 1) Controllers (API REST)
builder.Services.AddControllers();

// 2) Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3) DbContext (MySQL)
var conn = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<VentasDbContext>(options =>
    options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

var app = builder.Build();

// 4) Swagger solo en Development (recomendado para clase)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware básico
app.UseHttpsRedirection();
app.UseAuthorization();

// 5) Mapear Controllers
app.MapControllers();

app.Run();
