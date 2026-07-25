using System;

namespace Formix.Domain.Dtos
{
    public class ListaInmueblesByRadicadoDto
    {
        public int TenantId { get; set; }
        public int IdRadicado { get; set; }
        public string? Consecutivo { get; set; }
        public int InmuebleId { get; set; }
        public int ProyectoId { get; set; }
        public string Nombre { get; set; }
        public int? Orden { get; set; }
    }
}
