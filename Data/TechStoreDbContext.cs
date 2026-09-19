using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TiendaLinea.Models;

namespace TiendaLinea.Data;

public partial class TechStoreDbContext : DbContext
{
    public TechStoreDbContext(DbContextOptions<TechStoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Detallesventum> Detallesventa { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PRIMARY");

            entity.ToTable("categorias");

            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });

        modelBuilder.Entity<Detallesventum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("detallesventa");

            entity.HasIndex(e => e.ProductoCodigo, "FK_Detalle_Producto");

            entity.HasIndex(e => e.VentaCodigo, "FK_Detalle_Venta");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Cantidad).HasColumnType("int(11)");
            entity.Property(e => e.PrecioUnitario).HasPrecision(12, 2);
            entity.Property(e => e.ProductoCodigo).HasColumnType("int(11)");
            entity.Property(e => e.VentaCodigo).HasColumnType("int(11)");

            entity.HasOne(d => d.ProductoCodigoNavigation).WithMany(p => p.Detallesventa)
                .HasForeignKey(d => d.ProductoCodigo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detalle_Producto");

            entity.HasOne(d => d.VentaCodigoNavigation).WithMany(p => p.Detallesventa)
                .HasForeignKey(d => d.VentaCodigo)
                .HasConstraintName("FK_Detalle_Venta");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PRIMARY");

            entity.ToTable("productos");

            entity.Property(e => e.Codigo)
                .ValueGeneratedNever()
                .HasColumnType("int(11)");
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Precio).HasPrecision(12, 2);
            entity.Property(e => e.Stock).HasColumnType("int(11)");

            entity.HasMany(d => d.CategoriaCodigos).WithMany(p => p.ProductoCodigos)
                .UsingEntity<Dictionary<string, object>>(
                    "Productocategoria",
                    r => r.HasOne<Categoria>().WithMany()
                        .HasForeignKey("CategoriaCodigo")
                        .HasConstraintName("FK_PC_Categoria"),
                    l => l.HasOne<Producto>().WithMany()
                        .HasForeignKey("ProductoCodigo")
                        .HasConstraintName("FK_PC_Producto"),
                    j =>
                    {
                        j.HasKey("ProductoCodigo", "CategoriaCodigo")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("productocategorias");
                        j.HasIndex(new[] { "CategoriaCodigo" }, "FK_PC_Categoria");
                        j.IndexerProperty<int>("ProductoCodigo").HasColumnType("int(11)");
                        j.IndexerProperty<string>("CategoriaCodigo").HasMaxLength(50);
                    });
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "UX_Usuarios_Correo").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("int(11)");
            entity.Property(e => e.Cargo).HasMaxLength(100);
            entity.Property(e => e.ContactoDireccion)
                .HasMaxLength(300)
                .HasColumnName("Contacto_Direccion");
            entity.Property(e => e.ContactoTelefono)
                .HasMaxLength(20)
                .HasColumnName("Contacto_Telefono");
            entity.Property(e => e.Correo).HasMaxLength(200);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.NivelAcceso).HasColumnType("int(11)");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.UserType).HasMaxLength(15);
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PRIMARY");

            entity.ToTable("ventas");

            entity.HasIndex(e => e.ClienteId, "FK_Ventas_Cliente");

            entity.HasIndex(e => e.EmpleadoId, "FK_Ventas_Empleado");

            entity.Property(e => e.Codigo)
                .ValueGeneratedNever()
                .HasColumnType("int(11)");
            entity.Property(e => e.ClienteId).HasColumnType("int(11)");
            entity.Property(e => e.EmpleadoId).HasColumnType("int(11)");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Cliente).WithMany(p => p.VentaClientes)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Ventas_Cliente");

            entity.HasOne(d => d.Empleado).WithMany(p => p.VentaEmpleados)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Ventas_Empleado");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
