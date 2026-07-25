using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class TiposEstadoCivil
{
    public int IdEstadoCivil { get; set; }

    public string EstadoCivil { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Codigo { get; set; }
}
