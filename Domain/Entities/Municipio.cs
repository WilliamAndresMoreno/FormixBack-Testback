using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class Municipio
{
    public string CodigoDane { get; set; } = null!;

    public string NombreMunicipio { get; set; } = null!;

    public string CodigoDepartamento { get; set; } = null!;

    public string? TipoMunicipio { get; set; }

    public virtual Departamento CodigoDepartamentoNavigation { get; set; } = null!;
}
