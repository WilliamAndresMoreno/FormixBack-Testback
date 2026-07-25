using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class RadicadosOtorgantesTipo
{
    public int IdRadicadoOtorganteTipo { get; set; }

    public int IdRadicadoOtorgante { get; set; }

    public int IdTipoOtorgante { get; set; }

    public string? ActoCodigo { get; set; }

    public decimal? Porcentaje { get; set; }

    public int? AnioAdquisicion { get; set; }

    public string? CasaHabitacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual RadicadosOtorgante IdRadicadoOtorganteNavigation { get; set; } = null!;

    public virtual TiposOtorgante IdTipoOtorganteNavigation { get; set; } = null!;
}
