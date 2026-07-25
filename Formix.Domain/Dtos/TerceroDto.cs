using System;

namespace Formix.Domain.Dtos
{
    public partial class TerceroDto
    {
        public int IdTercero { get; set; }
        public string NombreCompleto { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? Documento { get; set; }
        public string? Correo { get; set; }
        public string? Celular { get; set; }
        public int? IdEstadoCivil { get; set; }
        public string? LugarExpedicionMunicipioCodigoDane { get; set; }
        public string? Direccion { get; set; }
        public int TenantId { get; set; }
    }
}
