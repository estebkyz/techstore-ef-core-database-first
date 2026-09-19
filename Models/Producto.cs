using System;
using System.Collections.Generic;

namespace TiendaLinea.Models;

public partial class Producto
{
    public int Codigo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public virtual ICollection<Detallesventum> Detallesventa { get; set; } = new List<Detallesventum>();

    public virtual ICollection<Categoria> CategoriaCodigos { get; set; } = new List<Categoria>();
}
