using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class RadicadosActo
{
    public int IdRadicado { get; set; }

    public int IdActo { get; set; }

    public decimal? Cuantia { get; set; }

    public decimal? Avaluo { get; set; }

    public DateOnly? FechaAdquisicion { get; set; }

    public virtual Acto IdActoNavigation { get; set; } = null!;

    public virtual Radicado IdRadicadoNavigation { get; set; } = null!;
}
