using System.ComponentModel.DataAnnotations;
namespace EjemploWebVentas.DTOs
{
    public class VentaDetalleCreateDto
    {
        [Range(1, int.MaxValue)]
        public int CarroId { get; set; }

        [Range(1, 1000)]
        public int Cantidad { get; set; }
    }
}
