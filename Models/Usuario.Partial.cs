using System;

namespace TiendaLinea.Models
{
    public partial class Usuario
    {
        public Usuario() { }

        // Fake properties para compilar la UI
        public string Clave { get; set; } = string.Empty;

        // Método vacío para compatibilidad con la exportación antigua
        public void SaveToJson(string path) { }
    }
}
