using System;
using System.Collections.Generic;

namespace TiendaLinea.Models;

public partial class Detallesventum
{
    public int Id { get; set; }

    public int VentaCodigo { get; set; }

    public int ProductoCodigo { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public virtual Producto ProductoCodigoNavigation { get; set; } = null!;

    public virtual Venta VentaCodigoNavigation { get; set; } = null!;
}
