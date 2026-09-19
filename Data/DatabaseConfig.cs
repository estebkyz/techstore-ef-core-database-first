using MySqlConnector;

namespace TiendaLinea.Data
{
    /// <summary>
    /// Reads MySQL credentials from a .env file (never from hard-coded strings) and
    /// builds a valid connection string.
    ///
    /// Two deliberate decisions:
    /// 1. Credentials live in .env, excluded by .gitignore — nothing secret is ever committed.
    ///    .env.example documents the required keys with fake values.
    /// 2. The connection string is assembled with MySqlConnectionStringBuilder instead of
    ///    string concatenation. Passwords routinely contain ; = / [ which have meaning inside
    ///    a connection string; the builder quotes and escapes them correctly.
    /// </summary>
    public static class DatabaseConfig
    {
        private static readonly object Gate = new();
        private static bool _envLoaded;

        /// <summary>
        /// Loads .env exactly once per process. Searches from AppContext.BaseDirectory upward,
        /// so it is found both from bin/Debug/net8.0-windows/ and from the EF Core CLI tooling.
        /// </summary>
        public static void EnsureEnvLoaded()
        {
            lock (Gate)
            {
                if (_envLoaded) return;

                var envPath = FindEnvFile();
                if (envPath is null)
                    throw new InvalidOperationException(
                        "No se encontró el archivo .env. " +
                        "Copie .env.example como .env en la raíz del proyecto y complete los valores.");

                DotNetEnv.Env.Load(envPath);
                _envLoaded = true;
            }
        }

        private static string? FindEnvFile()
        {
            foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var dir = new DirectoryInfo(start);
                while (dir is not null)
                {
                    var candidate = Path.Combine(dir.FullName, ".env");
                    if (File.Exists(candidate)) return candidate;
                    dir = dir.Parent;
                }
            }
            return null;
        }

        /// <summary>Builds the MySQL connection string from .env values.</summary>
        public static string GetConnectionString()
        {
            EnsureEnvLoaded();
            var builder = new MySqlConnectionStringBuilder
            {
                Server   = Require("DB_HOST"),
                Port     = uint.Parse(Require("DB_PORT")),
                Database = Require("DB_NAME"),
                UserID   = Require("DB_USER"),
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "",
                Pooling  = true
            };
            return builder.ConnectionString;
        }

        /// <summary>
        /// Same connection string but without Database=, used only for ServerVersion.AutoDetect
        /// (which needs to connect before the database is selected).
        /// </summary>
        public static string GetServerOnlyConnectionString()
        {
            EnsureEnvLoaded();
            var builder = new MySqlConnectionStringBuilder
            {
                Server   = Require("DB_HOST"),
                Port     = uint.Parse(Require("DB_PORT")),
                UserID   = Require("DB_USER"),
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? ""
            };
            return builder.ConnectionString;
        }

        public static string DatabaseName => Require("DB_NAME");

        private static string Require(string key)
        {
            EnsureEnvLoaded();
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Falta la clave '{key}' en el archivo .env.");
            return value;
        }
    }
}
