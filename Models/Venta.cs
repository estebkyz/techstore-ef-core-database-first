using System;
using System.Collections.Generic;

namespace TiendaLinea.Models;

public partial class Venta
{
    public int Codigo { get; set; }

    public DateTime Fecha { get; set; }

    public int? ClienteId { get; set; }

    public int? EmpleadoId { get; set; }

    public virtual Usuario? Cliente { get; set; }

    public virtual ICollection<Detallesventum> Detallesventa { get; set; } = new List<Detallesventum>();

    public virtual Usuario? Empleado { get; set; }
}
