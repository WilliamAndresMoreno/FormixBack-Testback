using System;

namespace Formix.Domain.Dtos
{
    public class TenantDto
    {
        public int TenantId { get; set; }
        public string CodigoNotaria { get; set; }
        public string NombreLegal { get; set; }
        public string? NombreComercial { get; set; }
        public string? Nit { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public bool? Activo { get; set; }
        public string? ConfiguracionJson { get; set; }
        public bool Consecutivo { get; set; }
    }
}
