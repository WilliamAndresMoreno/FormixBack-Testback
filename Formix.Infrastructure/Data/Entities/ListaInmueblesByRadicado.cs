using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class ListaInmueblesByRadicado
{
    public int TenantId { get; set; }

    public int IdRadicado { get; set; }

    public int Consecutivo { get; set; }

    public int InmuebleId { get; set; }

    public int ProyectoId { get; set; }

    public string Nombre { get; set; } = null!;

    public int? Orden { get; set; }
}
