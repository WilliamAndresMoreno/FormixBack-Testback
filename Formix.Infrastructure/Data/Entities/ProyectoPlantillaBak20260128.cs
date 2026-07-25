using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class ProyectoPlantillaBak20260128
{
    public int ProyectoId { get; set; }

    public int PlantillaId { get; set; }

    public string? Nombre { get; set; }

    public string? Archivo { get; set; }

    public int? Estado { get; set; }
}
