using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class TiposEstadoRadicado
{
    public int IdEstadoRadicado { get; set; }

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Radicado> Radicados { get; set; } = new List<Radicado>();
}
