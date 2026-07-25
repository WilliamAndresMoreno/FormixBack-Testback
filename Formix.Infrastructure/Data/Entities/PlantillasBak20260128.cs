using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class PlantillasBak20260128
{
    public int IdPlantilla { get; set; }

    public string Nombre { get; set; } = null!;

    public string Archivo { get; set; } = null!;

    public bool Estado { get; set; }
}
