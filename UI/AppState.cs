using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TiendaLinea.Data;
using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using TiendaLinea.Data.Context;
using System.Collections.Generic;
using System.Linq;

namespace TiendaLinea.UI
{
    public sealed class AppState
    {
        public BindingList<Producto> Productos { get; } = new();
        public BindingList<Usuario> Clientes { get; } = new();
        public BindingList<Usuario> Empleados { get; } = new();
        public BindingList<Usuario> Administradores { get; } = new();
        public BindingList<Venta> Ventas { get; } = new();

        public async Task LoadAllFromDatabaseAsync()
        {
            using var db = new AppDbContext();
            
            var prods = await db.Productos.OrderBy(p => p.Codigo).ToListAsync();
            var usrs = await db.Usuarios.OrderBy(u => u.Codigo).ToListAsync();
            var vents = await db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Detallesventa)
                    .ThenInclude(d => d.Producto)
                .OrderBy(v => v.Codigo)
                .ToListAsync();

            Productos.Clear();
            foreach (var p in prods) Productos.Add(p);

            Clientes.Clear();
            Empleados.Clear();
            Administradores.Clear();
            foreach (var u in usrs)
            {
                switch (u.TipoUsuario)
                {
                    case "Cliente": Clientes.Add(u); break;
                    case "Empleado": Empleados.Add(u); break;
                    case "Administrador": Administradores.Add(u); break;
                }
            }

            Ventas.Clear();
            foreach (var v in vents) Ventas.Add(v);
        }

        public int GetNextCodigoProducto() => Productos.Count > 0 ? Productos.Max(p => p.Codigo) + 1 : 101;
        
        public async Task AddProductoAsync(Producto producto)
        {
            using var db = new AppDbContext();
            db.Productos.Add(producto);
            await db.SaveChangesAsync();
            Productos.Add(producto);
        }

        public async Task UpdateProductoAsync(Producto producto)
        {
            using var db = new AppDbContext();
            var tracked = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == producto.Codigo);
            if (tracked != null)
            {
                tracked.Nombre = producto.Nombre;
                tracked.Categoria = producto.Categoria;
                tracked.Descripcion = producto.Descripcion;
                tracked.PrecioVenta = producto.PrecioVenta;
                tracked.StockActual = producto.StockActual;
                tracked.StockMinimo = producto.StockMinimo;
                tracked.Impuesto = producto.Impuesto;
                tracked.Activo = producto.Activo;
                await db.SaveChangesAsync();
            }
            Productos.ResetBindings();
        }
        
        public async Task<bool> TieneVentasAsociadasAsync(Producto producto)
        {
            using var db = new AppDbContext();
            return await db.Detallesventa.AnyAsync(d => d.ProductoId == producto.Codigo);
        }

        public int GetNextCodigoUsuario() => 
            Math.Max(
                Math.Max(
                    Clientes.Count > 0 ? Clientes.Max(c => c.Codigo) : 0,
                    Empleados.Count > 0 ? Empleados.Max(e => e.Codigo) : 0
                ),
                Administradores.Count > 0 ? Administradores.Max(a => a.Codigo) : 0
            ) + 1;

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            using var db = new AppDbContext();
            return await db.Usuarios.AnyAsync(u => u.Correo == correo.Trim().ToLower());
        }

        public async Task AddClienteAsync(Usuario c) { using var db = new AppDbContext(); db.Usuarios.Add(c); await db.SaveChangesAsync(); Clientes.Add(c); }
        public async Task UpdateClienteAsync(Usuario c) { using var db = new AppDbContext(); db.Usuarios.Update(c); await db.SaveChangesAsync(); Clientes.ResetBindings(); }

        public async Task AddEmpleadoAsync(Usuario e) { using var db = new AppDbContext(); db.Usuarios.Add(e); await db.SaveChangesAsync(); Empleados.Add(e); }
        public async Task UpdateEmpleadoAsync(Usuario e) { using var db = new AppDbContext(); db.Usuarios.Update(e); await db.SaveChangesAsync(); Empleados.ResetBindings(); }

        public async Task AddAdministradorAsync(Usuario a) { using var db = new AppDbContext(); db.Usuarios.Add(a); await db.SaveChangesAsync(); Administradores.Add(a); }
        public async Task UpdateAdministradorAsync(Usuario a) { using var db = new AppDbContext(); db.Usuarios.Update(a); await db.SaveChangesAsync(); Administradores.ResetBindings(); }

        // --- VENTAS ---
        public int GetNextCodigoVenta() => Ventas.Count > 0 ? Ventas.Max(v => v.Codigo) + 1 : 1001;

        public async Task AddVentaAsync(Venta venta, IList<DetalleVenta> detalles)
        {
            using var db = new AppDbContext();
            
            venta.Cliente = null!;
            venta.Empleado = null!;
            
            db.Ventas.Add(venta);
            foreach (var d in detalles)
            {
                d.Producto = null!;
                d.VentaId = venta.Codigo;
                db.Detallesventa.Add(d);
                
                var trackedProd = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == d.ProductoId);
                if (trackedProd != null) trackedProd.StockActual -= d.Cantidad;
            }
            
            await db.SaveChangesAsync();
            await LoadAllFromDatabaseAsync();
        }

        public string? ValidarStockParaVenta(IList<DetalleVenta> detalles)
        {
            foreach (var d in detalles)
            {
                var prod = Productos.FirstOrDefault(p => p.Codigo == d.ProductoId);
                if (prod != null && prod.StockActual < d.Cantidad)
                    return $@"'{prod.Nombre}' â€” disponible: {prod.StockActual}, requerido: {d.Cantidad}";
            }
            return null;
        }

        public async Task DeleteVentaAsync(Venta venta)
        {
            using var db = new AppDbContext();
            var tracked = await db.Ventas.FirstOrDefaultAsync(v => v.Codigo == venta.Codigo);
            if (tracked != null)
            {
                db.Ventas.Remove(tracked);
                await db.SaveChangesAsync();
            }
            Ventas.Remove(venta);
        }

        public async Task LoadDemoDataAsync()
        {
            using var db = new AppDbContext();
            
            await db.Detallesventa.ExecuteDeleteAsync();
            await db.Ventas.ExecuteDeleteAsync();
            await db.Productos.ExecuteDeleteAsync();
            await db.Usuarios.ExecuteDeleteAsync();

            var admin1 = new Usuario { Codigo = 1, Nombre = "Ana Martinez", Correo = "ana.admin@techstore.com", TipoUsuario = "Administrador" };
            var emp1 = new Usuario { Codigo = 2, Nombre = "Juan Perez", Correo = "juan.perez@techstore.com", TipoUsuario = "Empleado" };
            var cli1 = new Usuario { Codigo = 3, Nombre = "Carlos Lopez", Correo = "carlos@correo.com", TipoUsuario = "Cliente", Direccion = "Calle 100 #20-30" };

            var prod1 = new Producto { Codigo = 101, Nombre = "Laptop XPS 15", Categoria = "ComputaciÃ³n", Descripcion = "Laptop Dell 16GB RAM", PrecioVenta = 1500m, StockActual = 10, StockMinimo = 2, Impuesto = 0.19m, Activo = true };
            var prod2 = new Producto { Codigo = 102, Nombre = "Mouse InalÃ¡mbrico", Categoria = "Accesorios", Descripcion = "Mouse Ã³ptico", PrecioVenta = 25m, StockActual = 50, StockMinimo = 5, Impuesto = 0.19m, Activo = true };
            
            db.Usuarios.AddRange(admin1, emp1, cli1);
            db.Productos.AddRange(prod1, prod2);
            await db.SaveChangesAsync();

            var venta1 = new Venta { Codigo = 1001, ClienteId = cli1.Codigo, EmpleadoId = emp1.Codigo, FechaVenta = DateTime.Now.AddDays(-1) };
            db.Ventas.Add(venta1);
            
            var det1 = new DetalleVenta { VentaId = venta1.Codigo, ProductoId = prod1.Codigo, Cantidad = 1 };
            db.Detallesventa.Add(det1);
            prod1.StockActual -= 1;
            
            await db.SaveChangesAsync();
            await LoadAllFromDatabaseAsync();
        }
    }
}



