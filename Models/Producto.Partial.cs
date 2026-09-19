using System;

namespace TiendaLinea.Models
{
    public partial class Producto
    {
        public Producto() { }

        // Constructor parametrizado que la UI antigua usaba
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

        // Propiedad calculada que la UI espera para pintar la fila
        public bool StockBajo => StockActual <= StockMinimo;

        // Método vacío para compatibilidad con la exportación antigua
        public void SaveToJson(string path) { }
    }
}
