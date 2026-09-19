using System;

namespace TiendaLinea.Models
{
    public partial class Usuario
    {
        public Usuario() { }

        // Fake properties para compilar la UI
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Clave { get => ClaveHash; set => ClaveHash = value; }

        // Método vacío para compatibilidad con la exportación antigua
        public void SaveToJson(string path) { }
    }
}
