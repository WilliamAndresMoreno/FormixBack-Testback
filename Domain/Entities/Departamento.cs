using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class Departamento
{
    public string CodigoDane { get; set; } = null!;

    public string NombreDepartamento { get; set; } = null!;

    public string? NombreCorto { get; set; }

    public string? CodigoIso { get; set; }

    public virtual ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
}
