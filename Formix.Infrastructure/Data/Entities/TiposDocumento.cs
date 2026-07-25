using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class TiposDocumento
{
    public int IdTipoDocumento { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Sigla { get; set; }

    public string? Codigo { get; set; }

    public virtual ICollection<Tercero> Terceros { get; set; } = new List<Tercero>();
}
