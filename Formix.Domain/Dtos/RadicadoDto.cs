using System;

namespace Formix.Domain.Dtos
{
    public partial class RadicadoDto
    {
        public int? IdRadicado { get; set; }
        public int TenantId { get; set; }
        public int ProyectoId { get; set; }
        public int? PlantillaId { get; set; }
        public int UsuarioId { get; set; }
        public int Consecutivo { get; set; }
        public DateTime FechaRadicado { get; set; }
    }
}
