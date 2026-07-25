using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class Proyecto
{
    public int ProyectoId { get; set; }

    public int TenantId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string MunicipioCodigoDane { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string MatriculaInmobiliaria { get; set; } = null!;

    public string? LinderoGeneral { get; set; }

    public virtual ICollection<Inmueble> Inmuebles { get; set; } = new List<Inmueble>();

    public virtual Municipio MunicipioCodigoDaneNavigation { get; set; } = null!;

    public virtual ICollection<ProyectoPlantilla> ProyectoPlantillas { get; set; } = new List<ProyectoPlantilla>();

    public virtual ICollection<Radicado> Radicados { get; set; } = new List<Radicado>();

    public virtual Tenant Tenant { get; set; } = null!;
}
