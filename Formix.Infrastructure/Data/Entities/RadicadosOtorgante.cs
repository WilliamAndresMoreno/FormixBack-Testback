using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class RadicadosOtorgante
{
    public int IdRadicadoOtorgante { get; set; }

    public int IdTercero { get; set; }

    public int IdRadicado { get; set; }

    public virtual Radicado IdRadicadoNavigation { get; set; } = null!;

    public virtual Tercero IdTerceroNavigation { get; set; } = null!;

    public virtual ICollection<RadicadosOtorgantesTipo> RadicadosOtorgantesTipos { get; set; } = new List<RadicadosOtorgantesTipo>();
}
