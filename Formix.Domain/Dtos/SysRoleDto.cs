using System;

namespace Formix.Domain.Dtos
{
    public class SysRoleDto
    {
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? EsAdministrador { get; set; }
        public int? TenantId { get; set; }
        public bool? Activo { get; set; }
    }
}
