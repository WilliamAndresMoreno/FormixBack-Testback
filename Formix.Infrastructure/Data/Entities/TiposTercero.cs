using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class TiposTercero
{
    public int IdTipoTercero { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<TerceroTiposTercero> TerceroTiposTerceros { get; set; } = new List<TerceroTiposTercero>();
}
