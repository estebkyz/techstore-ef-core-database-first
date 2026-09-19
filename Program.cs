using System;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using TiendaLinea.Data.Context;
using TiendaLinea.UI;

namespace TiendaLinea
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var culture = new System.Globalization.CultureInfo("es-CO");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            // Crear/actualizar la base de datos MySQL automáticamente
            using (var context = new AppDbContext())
            {
                context.Database.Migrate();
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}


