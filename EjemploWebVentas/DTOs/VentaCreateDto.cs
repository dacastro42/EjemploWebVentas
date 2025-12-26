using System.ComponentModel.DataAnnotations;

namespace EjemploWebVentas.DTOs
{
    public class VentaCreateDto
    {
        [Range(1, int.MaxValue)]
        public int VendedorId { get; set; }

        [MinLength(1, ErrorMessage = "La venta debe tener al menos 1 detalle.")]
        public List<VentaDetalleCreateDto> Detalles { get; set; } = new();
    }
}
