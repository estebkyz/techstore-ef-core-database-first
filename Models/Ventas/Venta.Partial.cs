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
                public string GenerarTextoComprobante()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("             TECHSTORE S.A.             ");
            sb.AppendLine("========================================");
            sb.AppendLine("Factura #: " + Codigo);
            sb.AppendLine("Fecha    : " + FechaVenta.ToString("dd/MM/yyyy HH:mm"));
            if (Cliente != null) sb.AppendLine("Cliente  : " + Cliente.Nombre);
            if (Empleado != null) sb.AppendLine("Atiende  : " + Empleado.Nombre);
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("CANT  DESCRIPCION                 TOTAL ");
            sb.AppendLine("----------------------------------------");
            
            decimal subtotal = 0;
            decimal totalIva = 0;

            foreach (var d in Detallesventa)
            {
                string prodName = d.Producto?.Nombre ?? "Producto "+d.ProductoId;
                if (prodName.Length > 20) prodName = prodName.Substring(0, 20);
                string line = $"{d.Cantidad,-4} {prodName,-25} ";
                sb.AppendLine(line);
                
                subtotal += d.SubtotalItem;
                totalIva += d.IVAItem;
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"SUBTOTAL :                       ");
            sb.AppendLine($"IVA      :                       ");
            sb.AppendLine("========================================");
            sb.AppendLine($"TOTAL A PAGAR :                  ");
            sb.AppendLine("========================================");
            sb.AppendLine("      ¡GRACIAS POR SU COMPRA!           ");
            return sb.ToString();
        }
                public void SaveToJson(string path) { System.IO.File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })); }
    }
}





