using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class Proyecto
{
    public int ProyectoId { get; set; }

    public int TenantId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int MunicipioId { get; set; }

    public string Direccion { get; set; } = null!;

    public string MatriculaInmobiliaria { get; set; } = null!;

    public string? LinderoGeneral { get; set; }

    public virtual ICollection<Inmueble> Inmuebles { get; set; } = new List<Inmueble>();

    public virtual Tenant Tenant { get; set; } = null!;
}
