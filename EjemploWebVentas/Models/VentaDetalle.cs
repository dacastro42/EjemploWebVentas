namespace EjemploWebVentas.Models
{
    public class VentaDetalle
    {
        public int IdVentaDetalles { get; set; } // idventa_detalles

        public int VentaId { get; set; } // venta_id
        public Venta? Venta { get; set; }

        public int CarroId { get; set; } // carro_id
        public Carro? Carro { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
