using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

/// <summary>
/// Tipo de natu
/// </summary>
public partial class TiposOtorgante
{
    public int IdTipoOtorgante { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<InmublesTercero> InmublesTerceros { get; set; } = new List<InmublesTercero>();

    public virtual ICollection<RadicadosOtorgantesTipo> RadicadosOtorgantesTipos { get; set; } = new List<RadicadosOtorgantesTipo>();
}
