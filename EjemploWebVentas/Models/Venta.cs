namespace EjemploWebVentas.Models
{
    public class Venta
    {
        public int IsVentas { get; set; }   // <- PK real en MySQL

        public int VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }

        public DateTime FechaVenta { get; set; }
        // IVA en dinero (no tasa)
        public decimal Iva { get; set; }

        // Total con IVA incluido
        public decimal TotalVenta { get; set; }

        public List<VentaDetalle> Detalles { get; set; } = new();
    }
}
