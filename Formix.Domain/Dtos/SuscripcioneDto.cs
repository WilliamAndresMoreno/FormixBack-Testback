using System;

namespace Formix.Domain.Dtos
{
    public class SuscripcioneDto
    {
        public int SuscripcionId { get; set; }
        public int TenantId { get; set; }
        public int PlanId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaProximoPago { get; set; }
    }
}
