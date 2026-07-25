using System;

namespace Formix.Domain.Dtos
{
    public class SysAuditoriaLoginDto
    {
        public int AuditoriaId { get; set; }
        public int? UsuarioId { get; set; }
        public int? TenantId { get; set; }
        public DateTime? FechaHora { get; set; }
        public string? DireccionIp { get; set; }
        public string? Dispositivo { get; set; }
        public string? Navegador { get; set; }
        public bool? Exitoso { get; set; }
        public string? MensajeError { get; set; }
    }
}
