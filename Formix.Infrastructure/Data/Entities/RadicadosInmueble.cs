using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class RadicadosInmueble
{
    public int IdRadicado { get; set; }

    public int IdInmueble { get; set; }

    public int? Orden { get; set; }

    public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;

    public virtual Radicado IdRadicadoNavigation { get; set; } = null!;
}
