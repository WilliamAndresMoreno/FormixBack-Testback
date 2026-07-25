using System;

namespace Formix.Domain.Dtos
{
    public class SysUsuarioDto
    {
        public int UsuarioId { get; set; }
        public int TenantId { get; set; }
        public string NombreUsuario { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string NombreCompleto { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool? Bloqueado { get; set; }
        public int? IntentosLogin { get; set; }
    }
}
