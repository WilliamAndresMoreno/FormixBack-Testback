using System;

namespace Formix.Domain.Dtos
{
    public class PlaneDto
    {
        public int PlanId { get; set; }
        public string NombrePlan { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioMensual { get; set; }
        public int? MaxUsuarios { get; set; }
        public string? Caracteristicas { get; set; }
        public bool? Activo { get; set; }
    }
}
