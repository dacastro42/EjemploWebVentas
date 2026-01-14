using Microsoft.EntityFrameworkCore;
using EjemploWebVentas.Models;

namespace EjemploWebVentas.Data
{
    public class VentasDbContext : DbContext
    {
        public VentasDbContext(DbContextOptions<VentasDbContext> options) : base(options) { }

        public DbSet<Vendedor> Vendedores => Set<Vendedor>();
        public DbSet<Carro> Carros => Set<Carro>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TABLAS
            modelBuilder.Entity<Vendedor>().ToTable("vendedores");
            modelBuilder.Entity<Carro>().ToTable("carros");
            modelBuilder.Entity<Venta>().ToTable("ventas");
            modelBuilder.Entity<VentaDetalle>().ToTable("venta_detalles");

            // PKs + Columnas (para que coincida 1:1 con el diagrama)
            modelBuilder.Entity<Vendedor>(e =>
            {
                e.HasKey(x => x.IdV);
                e.Property(x => x.IdV).HasColumnName("idV");

                e.Property(x => x.Nombre1V).HasColumnName("nombre1V").HasMaxLength(45).IsRequired();
                e.Property(x => x.Nombre2V).HasColumnName("nombre2V").HasMaxLength(45);
                e.Property(x => x.Apellido1V).HasColumnName("apellido1V").HasMaxLength(45).IsRequired();
                e.Property(x => x.Apellido2V).HasColumnName("apellido2V").HasMaxLength(45);
                e.Property(x => x.EmailV).HasColumnName("emailV").HasMaxLength(45).IsRequired();
                e.Property(x => x.TelefonoV).HasColumnName("telefonoV").HasMaxLength(45);
                e.Property(x => x.PasswordV).HasColumnName("passwordV").HasMaxLength(45).IsRequired();

                e.Property(x => x.FechaRegistro)
                    .HasColumnName("fecha_registro")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                e.Property(x => x.Rol)
                    .HasColumnName("rol")
                    .HasMaxLength(20)
                    .HasDefaultValue("VENDEDOR")
                    .IsRequired();
            });

            modelBuilder.Entity<Carro>(e =>
            {
                e.HasKey(x => x.IdC);
                e.Property(x => x.IdC).HasColumnName("idC");

                e.Property(x => x.Marca).HasColumnName("marca").HasMaxLength(45).IsRequired();
                e.Property(x => x.Modelo).HasColumnName("modelo").HasMaxLength(45).IsRequired();
                e.Property(x => x.Anio).HasColumnName("anio");

                e.Property(x => x.PrecioC)
                    .HasColumnName("precioC")
                    .HasColumnType("decimal(12,2)");
            });

            modelBuilder.Entity<Venta>(e =>
            {
                e.HasKey(x => x.idVentas);
                e.Property(x => x.idVentas).HasColumnName("idVentas");

                e.Property(x => x.VendedorId).HasColumnName("vendedor_id");

                e.Property(x => x.FechaVenta)
                    .HasColumnName("fechaVenta")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();
                e.Property(x => x.Iva)
                    .HasColumnName("iva")
                    .HasColumnType("decimal(12,2)")
                    .HasDefaultValue(0);

                e.Property(x => x.TotalVenta)
                    .HasColumnName("totalVenta")
                    .HasColumnType("decimal(12,2)")
                    .HasDefaultValue(0);

                e.HasOne(x => x.Vendedor)
                 .WithMany(v => v.Ventas)
                 .HasForeignKey(x => x.VendedorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VentaDetalle>(e =>
            {
                e.HasKey(x => x.IdVentaDetalles);
                e.Property(x => x.IdVentaDetalles).HasColumnName("idventa_detalles");

                e.Property(x => x.VentaId).HasColumnName("venta_id");
                e.Property(x => x.CarroId).HasColumnName("carro_id");
                e.Property(x => x.Cantidad).HasColumnName("cantidad");

                e.Property(x => x.PrecioUnitario)
                 .HasColumnName("precio_unitario")
                 .HasColumnType("decimal(12,2)");

                e.Property(x => x.Subtotal)
                 .HasColumnName("subtotal")
                 .HasColumnType("decimal(12,2)");

                e.HasOne(x => x.Venta)
                 .WithMany(v => v.Detalles)
                 .HasForeignKey(x => x.VentaId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Carro)
                 .WithMany(c => c.VentaDetalles)
                 .HasForeignKey(x => x.CarroId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Único por email (recomendado)
            modelBuilder.Entity<Vendedor>()
                .HasIndex(v => v.EmailV)
                .IsUnique();
        }
    }
}
