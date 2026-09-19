using System;
using System.Collections.Generic;

namespace TiendaLinea.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Cargo { get; set; }

    public string? ContactoTelefono { get; set; }

    public string? ContactoDireccion { get; set; }

    public int? NivelAcceso { get; set; }

    public virtual ICollection<Venta> VentaClientes { get; set; } = new List<Venta>();

    public virtual ICollection<Venta> VentaEmpleados { get; set; } = new List<Venta>();
}
