using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class SysUsuarioRole
{
    public int UsuarioRolId { get; set; }

    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public virtual SysRole Rol { get; set; } = null!;

    public virtual SysUsuario Usuario { get; set; } = null!;
}
