using System.ComponentModel.DataAnnotations;
namespace EjemploWebVentas.Models
{
    public class Carro
    {
        public int IdC { get; set; } // idC

        [Required, StringLength(45)]
        public string Marca { get; set; } = string.Empty;

        [Required, StringLength(45)]
        public string Modelo { get; set; } = string.Empty;

        public int Anio { get; set; }

        public decimal PrecioC { get; set; }

        // 1 carro -> muchos detalles
        public List<VentaDetalle> VentaDetalles { get; set; } = new();
    }
}
