using System;
using System.Windows.Forms;
using TiendaLinea.UI;

namespace TiendaLinea
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
