using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class MinVariable
{
    public int VariableId { get; set; }

    public int CategoriaVariableId { get; set; }

    public string Variable { get; set; } = null!;

    public string? Codigo { get; set; }

    public bool Activo { get; set; }

    public virtual MinCategoriaVariable CategoriaVariable { get; set; } = null!;
}
