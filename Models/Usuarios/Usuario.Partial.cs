using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using System;

namespace TiendaLinea.Models.Usuarios
{
    public partial class Usuario
    {
        public Usuario() { }
        
                public void SaveToJson(string path) { System.IO.File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })); }
    }
}





