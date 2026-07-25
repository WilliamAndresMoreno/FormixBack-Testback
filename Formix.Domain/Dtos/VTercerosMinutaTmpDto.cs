using System;

namespace Formix.Domain.Dtos
{
    public class VTercerosMinutaTmpDto
    {
        public int InmuebleId { get; set; }
        public int TenantId { get; set; }
        public int ProyectoId { get; set; }
        public string Inmueble { get; set; }
        public string? CompradorNombreCompleto { get; set; }
        public string CompradorTipoDocumento { get; set; }
        public string? CompradorNroDocumento { get; set; }
        public string? Correo { get; set; }
        public string? Celular { get; set; }
    }
}
