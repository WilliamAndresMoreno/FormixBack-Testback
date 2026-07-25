using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class SysAuditoriaLogin
{
    public int AuditoriaId { get; set; }

    public int? UsuarioId { get; set; }

    public int? TenantId { get; set; }

    public DateTime? FechaHora { get; set; }

    public string? DireccionIp { get; set; }

    public string? Dispositivo { get; set; }

    public string? Navegador { get; set; }

    public bool? Exitoso { get; set; }

    public string? MensajeError { get; set; }

    public virtual Tenant? Tenant { get; set; }

    public virtual SysUsuario? Usuario { get; set; }
}
