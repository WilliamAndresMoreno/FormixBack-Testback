using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class SysRole
{
    public int RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? EsAdministrador { get; set; }

    public int? TenantId { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<SysRolPermiso> SysRolPermisos { get; set; } = new List<SysRolPermiso>();

    public virtual ICollection<SysUsuarioRole> SysUsuarioRoles { get; set; } = new List<SysUsuarioRole>();

    public virtual Tenant? Tenant { get; set; }
}
