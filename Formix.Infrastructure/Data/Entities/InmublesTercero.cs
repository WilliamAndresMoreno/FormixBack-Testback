using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class InmublesTercero
{
    public int IdTercero { get; set; }

    public int IdInmueble { get; set; }

    public int? IdTipoOtorgante { get; set; }

    public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;

    public virtual Tercero IdTerceroNavigation { get; set; } = null!;

    public virtual TiposOtorgante? IdTipoOtorganteNavigation { get; set; }
}
