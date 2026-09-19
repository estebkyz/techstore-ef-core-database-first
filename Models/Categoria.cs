using System;
using System.Collections.Generic;

namespace TiendaLinea.Models;

public partial class Categoria
{
    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Producto> ProductoCodigos { get; set; } = new List<Producto>();
}
