using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using TiendaLinea.Data;
using TiendaLinea.Models;

namespace TiendaLinea.UI
{
    public sealed class AppState
    {
        public BindingList<Usuario>  Clientes        { get; } = new();
        public BindingList<Usuario>  Empleados       { get; } = new();
        public BindingList<Usuario>  Administradores { get; } = new();
        public BindingList<Producto> Productos       { get; } = new();
        public BindingList<Venta>    Ventas          { get; } = new();
        public BindingList<Categoria> Categorias     { get; } = new();

        public void Clear()
        {
            Clientes.Clear();
            Empleados.Clear();
            Administradores.Clear();
            Productos.Clear();
            Ventas.Clear();
            Categorias.Clear();
        }

        public async Task LoadAllFromDatabaseAsync()
        {
            using var db = new TechStoreDbContext();

            var usuarios = await db.Usuarios.OrderBy(u => u.Id).ToListAsync();
            var productos = await db.Productos
                .Include(p => p.CategoriaCodigos)
                .OrderBy(p => p.Codigo)
                .ToListAsync();
            var ventas = await db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Detallesventa)
                    .ThenInclude(d => d.ProductoCodigoNavigation)
                .OrderBy(v => v.Codigo)
                .ToListAsync();
            var categorias = await db.Categorias.OrderBy(c => c.Codigo).ToListAsync();

            Clear();
            foreach (var u in usuarios)
            {
                switch (u.UserType)
                {
                    case UserTypes.Cliente:       Clientes.Add(u);        break;
                    case UserTypes.Empleado:      Empleados.Add(u);       break;
                    case UserTypes.Administrador: Administradores.Add(u); break;
                }
            }
            foreach (var p in productos)  Productos.Add(p);
            foreach (var v in ventas)     Ventas.Add(v);
            foreach (var c in categorias) Categorias.Add(c);
        }

        public async Task AddClienteAsync(Usuario cliente)
        {
            using var db = new TechStoreDbContext();
            db.Usuarios.Add(cliente);
            await db.SaveChangesAsync();
            Clientes.Add(cliente);
        }

        public async Task UpdateClienteAsync(Usuario cliente)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == cliente.Id);
            if (tracked is not null)
            {
                tracked.Nombre    = cliente.Nombre;
                tracked.Correo    = cliente.Correo;
                tracked.Direccion = cliente.Direccion;
                await db.SaveChangesAsync();
            }
            Clientes.ResetBindings();
        }

        public async Task DeleteClienteAsync(Usuario cliente)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == cliente.Id);
            if (tracked is not null)
            {
                db.Usuarios.Remove(tracked);
                await db.SaveChangesAsync();
            }
            await LoadAllFromDatabaseAsync();
        }

        public async Task AddEmpleadoAsync(Usuario empleado)
        {
            using var db = new TechStoreDbContext();
            db.Usuarios.Add(empleado);
            await db.SaveChangesAsync();
            Empleados.Add(empleado);
        }

        public async Task UpdateEmpleadoAsync(Usuario empleado)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == empleado.Id);
            if (tracked is not null)
            {
                tracked.Nombre             = empleado.Nombre;
                tracked.Correo             = empleado.Correo;
                tracked.Cargo              = empleado.Cargo;
                tracked.ContactoTelefono   = empleado.ContactoTelefono;
                tracked.ContactoDireccion  = empleado.ContactoDireccion;
                await db.SaveChangesAsync();
            }
            Empleados.ResetBindings();
        }

        public async Task DeleteEmpleadoAsync(Usuario empleado)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == empleado.Id);
            if (tracked is not null)
            {
                db.Usuarios.Remove(tracked);
                await db.SaveChangesAsync();
            }
            await LoadAllFromDatabaseAsync();
        }

        public async Task AddAdminAsync(Usuario admin)
        {
            using var db = new TechStoreDbContext();
            db.Usuarios.Add(admin);
            await db.SaveChangesAsync();
            Administradores.Add(admin);
        }

        public async Task UpdateAdminAsync(Usuario admin)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == admin.Id);
            if (tracked is not null)
            {
                tracked.Nombre       = admin.Nombre;
                tracked.Correo       = admin.Correo;
                tracked.NivelAcceso  = admin.NivelAcceso;
                await db.SaveChangesAsync();
            }
            Administradores.ResetBindings();
        }

        public async Task DeleteAdminAsync(Usuario admin)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == admin.Id);
            if (tracked is not null)
            {
                db.Usuarios.Remove(tracked);
                await db.SaveChangesAsync();
            }
            Administradores.Remove(admin);
        }

        public async Task AddCategoriaAsync(Categoria categoria)
        {
            using var db = new TechStoreDbContext();
            db.Categorias.Add(categoria);
            await db.SaveChangesAsync();
            Categorias.Add(categoria);
        }

        public async Task UpdateCategoriaAsync(Categoria categoria)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Categorias.FirstOrDefaultAsync(c => c.Codigo == categoria.Codigo);
            if (tracked is not null)
            {
                tracked.Nombre = categoria.Nombre;
                await db.SaveChangesAsync();
            }
            Categorias.ResetBindings();
        }

        public async Task DeleteCategoriaAsync(Categoria categoria)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Categorias.FirstOrDefaultAsync(c => c.Codigo == categoria.Codigo);
            if (tracked is not null)
            {
                db.Categorias.Remove(tracked);
                await db.SaveChangesAsync();
            }
            await LoadAllFromDatabaseAsync();
        }

        public async Task AddProductoAsync(Producto producto)
        {
            using var db = new TechStoreDbContext();
            db.Productos.Add(producto);
            await db.SaveChangesAsync();
            Productos.Add(producto);
        }

        public async Task UpdateProductoAsync(Producto producto)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == producto.Codigo);
            if (tracked is not null)
            {
                tracked.Nombre      = producto.Nombre;
                tracked.Descripcion = producto.Descripcion;
                tracked.Precio      = producto.Precio;
                tracked.Stock       = producto.Stock;
                await db.SaveChangesAsync();
            }
            Productos.ResetBindings();
        }

        public async Task DeleteProductoAsync(Producto producto)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Productos.FirstOrDefaultAsync(p => p.Codigo == producto.Codigo);
            if (tracked is not null)
            {
                db.Productos.Remove(tracked);
                await db.SaveChangesAsync();
            }
            await LoadAllFromDatabaseAsync();
        }

        public async Task AddCategoriaToProductoAsync(Producto producto, Categoria categoria)
        {
            using var db = new TechStoreDbContext();
            var trackedProd = await db.Productos
                .Include(p => p.CategoriaCodigos)
                .FirstOrDefaultAsync(p => p.Codigo == producto.Codigo)
                ?? throw new InvalidOperationException($"El producto {producto.Codigo} ya no existe.");
            var trackedCat = await db.Categorias.FirstOrDefaultAsync(c => c.Codigo == categoria.Codigo)
                ?? throw new InvalidOperationException($"La categoría {categoria.Codigo} ya no existe.");

            if (trackedProd.CategoriaCodigos.All(c => c.Codigo != trackedCat.Codigo))
            {
                trackedProd.CategoriaCodigos.Add(trackedCat);
                await db.SaveChangesAsync();
            }

            if (producto.CategoriaCodigos.All(c => c.Codigo != categoria.Codigo))
                producto.CategoriaCodigos.Add(categoria);

            Productos.ResetBindings();
        }

        public async Task AddVentaAsync(Venta venta, IEnumerable<Detallesventum> detalles)
        {
            using var db = new TechStoreDbContext();
            db.Ventas.Add(venta);
            foreach (var d in detalles)
            {
                d.VentaCodigo = venta.Codigo;
                db.Detallesventa.Add(d);
            }
            await db.SaveChangesAsync();
            await LoadAllFromDatabaseAsync();
        }

        public async Task DeleteVentaAsync(Venta venta)
        {
            using var db = new TechStoreDbContext();
            var tracked = await db.Ventas.FirstOrDefaultAsync(v => v.Codigo == venta.Codigo);
            if (tracked is not null)
            {
                db.Ventas.Remove(tracked);
                await db.SaveChangesAsync();
            }
            Ventas.Remove(venta);
        }

        public async Task LoadDemoDataAsync()
        {
            await ClearDatabaseAsync();
            Clear();

            var cli1 = new Usuario { Id = 1, Nombre = "Carlos Lopez",  Correo = "carlos@correo.com",        UserType = UserTypes.Cliente,       Direccion = "Calle 100 #20-30" };
            var cli2 = new Usuario { Id = 2, Nombre = "María García",  Correo = "maria@correo.com",         UserType = UserTypes.Cliente,       Direccion = "Av. 5 #10-15" };
            var emp1 = new Usuario { Id = 3, Nombre = "Juan Pérez",    Correo = "juan.perez@techstore.com", UserType = UserTypes.Empleado,      Cargo = "Vendedor",    ContactoTelefono = "3001234567", ContactoDireccion = "Calle 50 #30" };
            var adm1 = new Usuario { Id = 4, Nombre = "Ana Martínez",  Correo = "ana.admin@techstore.com",  UserType = UserTypes.Administrador, NivelAcceso = 5 };

            await AddClienteAsync(cli1);
            await AddClienteAsync(cli2);
            await AddEmpleadoAsync(emp1);
            await AddAdminAsync(adm1);

            var catComp  = new Categoria { Codigo = "COMP",  Nombre = "Computación" };
            var catAcc   = new Categoria { Codigo = "ACC",   Nombre = "Accesorios" };
            var catAudio = new Categoria { Codigo = "AUDIO", Nombre = "Audio" };

            await AddCategoriaAsync(catComp);
            await AddCategoriaAsync(catAcc);
            await AddCategoriaAsync(catAudio);

            var prod1 = new Producto { Codigo = 101, Nombre = "Laptop XPS 15",        Descripcion = "Dell 16GB RAM, SSD 512GB", Precio = 1500m, Stock = 10 };
            var prod2 = new Producto { Codigo = 102, Nombre = "Mouse Inalámbrico",    Descripcion = "Mouse óptico inalámbrico", Precio = 25m,   Stock = 50 };
            var prod3 = new Producto { Codigo = 103, Nombre = "Teclado Mecánico RGB", Descripcion = "Teclado mecánico gamer",   Precio = 75m,   Stock = 30 };

            await AddProductoAsync(prod1);
            await AddProductoAsync(prod2);
            await AddProductoAsync(prod3);

            await AddCategoriaToProductoAsync(prod1, catComp);
            await AddCategoriaToProductoAsync(prod2, catAcc);
            await AddCategoriaToProductoAsync(prod3, catAcc);

            var venta1 = new Venta { Codigo = 1001, Fecha = DateTime.Now.AddDays(-1), ClienteId = cli1.Id, EmpleadoId = emp1.Id };
            var detalles1 = new List<Detallesventum>
            {
                new() { ProductoCodigo = prod1.Codigo, Cantidad = 1, PrecioUnitario = prod1.Precio },
                new() { ProductoCodigo = prod2.Codigo, Cantidad = 2, PrecioUnitario = prod2.Precio }
            };
            await AddVentaAsync(venta1, detalles1);
        }

        public static async Task ClearDatabaseAsync()
        {
            using var db = new TechStoreDbContext();
            await db.Detallesventa.ExecuteDeleteAsync();
            await db.Ventas.ExecuteDeleteAsync();
            await db.Productos.ExecuteDeleteAsync();
            await db.Categorias.ExecuteDeleteAsync();
            await db.Usuarios.ExecuteDeleteAsync();
        }
    }
}
