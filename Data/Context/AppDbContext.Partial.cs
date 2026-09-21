using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using Microsoft.EntityFrameworkCore;
using System;

namespace TiendaLinea.Data.Context
{
    public partial class AppDbContext
    {
        private static readonly Lazy<ServerVersion> CachedServerVersion = new(() =>
            ServerVersion.AutoDetect(DatabaseConfig.GetServerOnlyConnectionString()));

        public AppDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            optionsBuilder.UseMySql(
                DatabaseConfig.GetConnectionString(),
                CachedServerVersion.Value);
        }
    }
}


