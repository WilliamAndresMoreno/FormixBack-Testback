using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class ListaRadicado
{
    public int IdRadicado { get; set; }

    public int TenantId { get; set; }

    public int ProyectoId { get; set; }

    public int? PlantillaId { get; set; }

    public int UsuarioId { get; set; }

    public int Consecutivo { get; set; }

    public DateTime FechaRadicado { get; set; }

    public string Proyecto { get; set; } = null!;
}
