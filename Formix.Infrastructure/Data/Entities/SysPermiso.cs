using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class SysPermiso
{
    public int PermisoId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<SysRolPermiso> SysRolPermisos { get; set; } = new List<SysRolPermiso>();
}
