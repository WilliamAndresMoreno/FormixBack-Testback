using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class SysRolPermiso
{
    public int RolPermisoId { get; set; }

    public int RolId { get; set; }

    public int PermisoId { get; set; }

    public virtual SysPermiso Permiso { get; set; } = null!;

    public virtual SysRole Rol { get; set; } = null!;
}
