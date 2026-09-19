using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using Microsoft.EntityFrameworkCore;

namespace TiendaLinea.Data.Context
{
    /// <summary>
    /// Hand-written half of <see cref="AppDbContext"/>. Kept in a SEPARATE file on purpose:
    /// AppDbContext.cs is regenerated every time you run
    /// <c>dotnet ef dbcontext scaffold ... --force</c> after a schema change, and that command
    /// overwrites the file completely. Anything hand-written inside it would be silently discarded
    /// on the next scaffold. A second partial class file with a different name is never touched by
    /// scaffolding, so it is the right place for:
    ///
    /// 1. The parameterless constructor + <see cref="OnConfiguring"/> pattern EF Core documents for
    ///    apps without a dependency-injection container (this is a WinForms desktop app — see
    ///    <c>UI/AppState.cs</c>, which does <c>using var db = new AppDbContext();</c> once per
    ///    operation).
    /// 2. Reading the connection string from .env via <see cref="DatabaseConfig"/> instead of the
    ///    plaintext connection string scaffolding would otherwise write directly into
    ///    AppDbContext.cs (scaffold warns about this on the console when you omit
    ///    <c>--no-onconfiguring</c> — the option this project always scaffolds with).
    /// </summary>
    public partial class AppDbContext
    {
        // ServerVersion.AutoDetect opens a connection to ask MySQL what version it is. Doing that
        // for every short-lived context would be wasteful, so the answer is cached for the process.
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


