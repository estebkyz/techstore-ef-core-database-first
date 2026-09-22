using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using System;

namespace TiendaLinea.Models.Inventario
{
    public partial class Producto
    {
        public Producto() { }

        public Producto(int codigo, string nombre, string categoria, string descripcion, decimal precioVenta, int stockActual, int stockMinimo, decimal impuesto, bool activo)
        {
            Codigo = codigo;
            Nombre = nombre;
            Categoria = categoria;
            Descripcion = descripcion;
            PrecioVenta = precioVenta;
            StockActual = stockActual;
            StockMinimo = stockMinimo;
            Impuesto = impuesto;
            Activo = activo;
        }

        public bool StockBajo => StockActual <= StockMinimo;

                public void SaveToJson(string path) { System.IO.File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })); }
    }
}




