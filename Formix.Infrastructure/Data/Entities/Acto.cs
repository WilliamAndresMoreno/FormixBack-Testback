using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class Acto
{
    public int IdActo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Abreviatura { get; set; }

    public int? Estado { get; set; }

    public virtual ICollection<RadicadosActo> RadicadosActos { get; set; } = new List<RadicadosActo>();
}
