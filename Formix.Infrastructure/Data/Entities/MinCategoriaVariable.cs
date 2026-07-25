using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class MinCategoriaVariable
{
    public int CategoriaVariableId { get; set; }

    public string Categoria { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<MinVariable> MinVariables { get; set; } = new List<MinVariable>();
}
