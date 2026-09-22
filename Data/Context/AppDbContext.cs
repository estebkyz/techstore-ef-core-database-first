using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TiendaLinea.Models;

namespace TiendaLinea.Data.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DetalleVenta> Detallesventa { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("detallesventa")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => e.ProductoId, "IX_DetallesVenta_ProductoId");

            entity.HasIndex(e => e.VentaId, "IX_DetallesVenta_VentaId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Cantidad).HasColumnType("int(11)");
            entity.Property(e => e.ProductoId).HasColumnType("int(11)");
            entity.Property(e => e.VentaId).HasColumnType("int(11)");

            entity.HasOne(d => d.Producto).WithMany(p => p.Detallesventa)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallesVenta_Productos_ProductoId");

            entity.HasOne(d => d.Venta).WithMany(p => p.Detallesventa)
                .HasForeignKey(d => d.VentaId)
                .HasConstraintName("FK_DetallesVenta_Ventas_VentaId");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PRIMARY");

            entity
                .ToTable("productos")
                .UseCollation("utf8mb4_general_ci");

            entity.Property(e => e.Codigo)
                .ValueGeneratedNever()
                .HasColumnType("int(11)");
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Impuesto).HasPrecision(5, 4);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.PrecioVenta).HasPrecision(12, 2);
            entity.Property(e => e.StockActual).HasColumnType("int(11)");
            entity.Property(e => e.StockMinimo).HasColumnType("int(11)");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PRIMARY");

            entity
                .ToTable("usuarios")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => e.Correo, "IX_Usuarios_Correo").IsUnique();

            entity.Property(e => e.Codigo)
                .ValueGeneratedNever()
                .HasColumnType("int(11)");
            entity.Property(e => e.ClaveHash).HasMaxLength(128);
            entity.Property(e => e.Comuna).HasMaxLength(45);
            entity.Property(e => e.Correo).HasMaxLength(200);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.TipoUsuario).HasMaxLength(13);
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PRIMARY");

            entity
                .ToTable("ventas")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => e.ClienteId, "IX_Ventas_ClienteId");

            entity.HasIndex(e => e.EmpleadoId, "IX_Ventas_EmpleadoId");

            entity.Property(e => e.Codigo)
                .ValueGeneratedNever()
                .HasColumnType("int(11)");
            entity.Property(e => e.ClienteId).HasColumnType("int(11)");
            entity.Property(e => e.EmpleadoId).HasColumnType("int(11)");
            entity.Property(e => e.FechaVenta).HasMaxLength(6);

            entity.HasOne(d => d.Cliente).WithMany(p => p.VentaClientes)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Usuarios_ClienteId");

            entity.HasOne(d => d.Empleado).WithMany(p => p.VentaEmpleados)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Usuarios_EmpleadoId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}




