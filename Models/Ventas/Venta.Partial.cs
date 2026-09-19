using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using System;
using System.Linq;

namespace TiendaLinea.Models.Ventas
{
    public partial class Venta
    {
        public Venta() {}
        public Venta(int codigo, Usuario cliente, Usuario empleado, DateTime fecha)
        {
            Codigo = codigo;
            Cliente = cliente;
            ClienteId = cliente?.Codigo ?? 0;
            Empleado = empleado;
            EmpleadoId = empleado?.Codigo ?? 0;
            FechaVenta = fecha;
        }

        public decimal CalcularTotal() => Detallesventa.Sum(d => d.TotalItem);
        public string GenerarTextoComprobante() => $"Comprobante Venta #{Codigo}";
        public void SaveToJson(string path) { }
    }
}



