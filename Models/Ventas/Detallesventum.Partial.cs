using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using System;
using System.Linq;

namespace TiendaLinea.Models.Ventas
{
    public partial class Detallesventum
    {
        public Detallesventum() {}
        public Detallesventum(Producto p, int cantidad)
        {
            Producto = p;
            ProductoId = p.Codigo;
            Cantidad = cantidad;
        }

        public decimal SubtotalItem => Producto?.PrecioVenta * Cantidad ?? 0;
        public decimal IVAItem => SubtotalItem * (Producto?.Impuesto ?? 0);
        public decimal TotalItem => SubtotalItem + IVAItem;
    }
}



