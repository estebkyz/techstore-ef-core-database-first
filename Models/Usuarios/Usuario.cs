using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using System;
using System.Collections.Generic;

namespace TiendaLinea.Models.Usuarios;

public partial class Usuario
{
    public int Codigo { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string ClaveHash { get; set; } = null!;

    public bool Activo { get; set; }

    public string TipoUsuario { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Comuna { get; set; }

    public virtual ICollection<Venta> VentaClientes { get; set; } = new List<Venta>();

    public virtual ICollection<Venta> VentaEmpleados { get; set; } = new List<Venta>();
}




