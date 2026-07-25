using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class ListaTercero
{
    public int TenantId { get; set; }

    public int IdTercero { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public int? IdTipoDocumento { get; set; }

    public string? Documento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? Correo { get; set; }

    public string? Celular { get; set; }
}
