using System.ComponentModel.DataAnnotations;
namespace EjemploWebVentas.Models
{
    public class Vendedor
    {
        public int IdV { get; set; } // idV

        [Required, StringLength(45)]
        public string Nombre1V { get; set; } = string.Empty;

        [StringLength(45)]
        public string? Nombre2V { get; set; }

        [Required, StringLength(45)]
        public string Apellido1V { get; set; } = string.Empty;

        [StringLength(45)]
        public string? Apellido2V { get; set; }

        [Required, StringLength(45), EmailAddress]
        public string EmailV { get; set; } = string.Empty;

        [StringLength(45)]
        public string? TelefonoV { get; set; }

        [Required, StringLength(45)]
        public string PasswordV { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        // 1 vendedor -> muchas ventas
        public List<Venta> Ventas { get; set; } = new();
    }
}
