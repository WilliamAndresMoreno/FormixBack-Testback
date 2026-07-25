using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class TiposInmuebleHomologación
{
    public int IdTipoInmuebleHomologado { get; set; }

    public int IdTercero { get; set; }

    public int TipoInmuebleId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual Tercero IdTerceroNavigation { get; set; } = null!;

    public virtual TipoInmueble TipoInmueble { get; set; } = null!;
}
