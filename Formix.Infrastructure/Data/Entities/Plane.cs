using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class Plane
{
    public int PlanId { get; set; }

    public string NombrePlan { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal? PrecioMensual { get; set; }

    public int? MaxUsuarios { get; set; }

    public string? Caracteristicas { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Suscripcione> Suscripciones { get; set; } = new List<Suscripcione>();
}
