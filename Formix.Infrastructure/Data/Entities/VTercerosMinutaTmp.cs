using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class VTercerosMinutaTmp
{
    public int InmuebleId { get; set; }

    public int TenantId { get; set; }

    public int ProyectoId { get; set; }

    public string Inmueble { get; set; } = null!;

    public string? CompradorNombreCompleto { get; set; }

    public string CompradorTipoDocumento { get; set; } = null!;

    public string? CompradorNroDocumento { get; set; }

    public string? Correo { get; set; }

    public string? Celular { get; set; }
}
