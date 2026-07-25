using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class CajasCompensacion
{
    public int IdCajaCompensacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Codigo { get; set; }

    public int? IdTipoDocumento { get; set; }

    public string? Documento { get; set; }

    public string? Correo { get; set; }

    public string? Celular { get; set; }

    public string? Direccion { get; set; }

    public virtual ICollection<RadicadosPago> RadicadosPagos { get; set; } = new List<RadicadosPago>();
}
