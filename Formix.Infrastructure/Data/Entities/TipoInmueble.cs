using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class TipoInmueble
{
    public int TipoInmuebleId { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Inmueble> Inmuebles { get; set; } = new List<Inmueble>();

    public virtual ICollection<TiposInmuebleHomologación> TiposInmuebleHomologacións { get; set; } = new List<TiposInmuebleHomologación>();
}
