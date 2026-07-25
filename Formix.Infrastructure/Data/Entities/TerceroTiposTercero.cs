using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class TerceroTiposTercero
{
    public int IdTerceroTiposTercero { get; set; }

    public int IdTercero { get; set; }

    public int IdTipoTercero { get; set; }

    public virtual Tercero IdTerceroNavigation { get; set; } = null!;

    public virtual TiposTercero IdTipoTerceroNavigation { get; set; } = null!;
}
