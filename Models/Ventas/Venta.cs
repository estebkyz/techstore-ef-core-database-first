using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using System;
using System.Collections.Generic;

namespace TiendaLinea.Models.Ventas;

public partial class Venta
{
    public int Codigo { get; set; }

    public int ClienteId { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime FechaVenta { get; set; }

    public virtual Usuario Cliente { get; set; } = null!;

    public virtual ICollection<Detallesventum> Detallesventa { get; set; } = new List<Detallesventum>();

    public virtual Usuario Empleado { get; set; } = null!;
}


