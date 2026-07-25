using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class SysConfiguracionSistema
{
    public int ConfiguracionId { get; set; }

    public string Clave { get; set; } = null!;

    public string? Valor { get; set; }

    public string? Descripcion { get; set; }

    public bool? EsSensible { get; set; }
}
