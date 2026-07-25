using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class SysUsuario
{
    public int UsuarioId { get; set; }

    public int TenantId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public string NombreCompleto { get; set; } = null!;

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public DateTime? UltimoLogin { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public bool? Bloqueado { get; set; }

    public int? IntentosLogin { get; set; }

    public virtual ICollection<SysAuditoriaLogin> SysAuditoriaLogins { get; set; } = new List<SysAuditoriaLogin>();

    public virtual ICollection<SysUsuarioRole> SysUsuarioRoles { get; set; } = new List<SysUsuarioRole>();

    public virtual Tenant Tenant { get; set; } = null!;
}
