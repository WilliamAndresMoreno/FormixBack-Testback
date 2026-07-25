using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class Plantilla
{
    public int IdPlantilla { get; set; }

    public string Nombre { get; set; } = null!;

    public string Archivo { get; set; } = null!;

    public bool Estado { get; set; }

    public virtual ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
