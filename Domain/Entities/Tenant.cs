using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class Tenant
{
    public int TenantId { get; set; }

    public string CodigoNotaria { get; set; } = null!;

    public string NombreLegal { get; set; } = null!;

    public string? NombreComercial { get; set; }

    public string? Nit { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Activo { get; set; }

    public string? ConfiguracionJson { get; set; }

    public bool Consecutivo { get; set; }

    public virtual ICollection<Inmueble> Inmuebles { get; set; } = new List<Inmueble>();

    public virtual ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();

    public virtual ICollection<Suscripcione> Suscripciones { get; set; } = new List<Suscripcione>();

    public virtual ICollection<SysAuditoriaLogin> SysAuditoriaLogins { get; set; } = new List<SysAuditoriaLogin>();

    public virtual ICollection<SysRole> SysRoles { get; set; } = new List<SysRole>();

    public virtual ICollection<SysUsuario> SysUsuarios { get; set; } = new List<SysUsuario>();
}
