using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class Suscripcione
{
    public int SuscripcionId { get; set; }

    public int TenantId { get; set; }

    public int PlanId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaProximoPago { get; set; }

    public virtual Plane Plan { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
