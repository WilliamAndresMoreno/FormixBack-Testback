using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class TipoInmueble
{
    public int TipoInmuebleId { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Inmueble> Inmuebles { get; set; } = new List<Inmueble>();
}
