using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class ProyectoPlantilla
{
    public int PlantillaId { get; set; }

    public int ProyectoId { get; set; }

    public string? Nombre { get; set; }

    public string? Archivo { get; set; }

    public int Estado { get; set; }

    public virtual Proyecto Proyecto { get; set; } = null!;

    public virtual ICollection<Radicado> Radicados { get; set; } = new List<Radicado>();
}
