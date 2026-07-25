using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class Radicado
{
    public int IdRadicado { get; set; }

    public int TenantId { get; set; }

    public int ProyectoId { get; set; }

    public int? PlantillaId { get; set; }

    public int UsuarioId { get; set; }

    public int Consecutivo { get; set; }

    public DateTime FechaRadicado { get; set; }

    public virtual ProyectoPlantilla? Plantilla { get; set; }

    public virtual Proyecto Proyecto { get; set; } = null!;

    public virtual ICollection<RadicadosActo> RadicadosActos { get; set; } = new List<RadicadosActo>();

    public virtual ICollection<RadicadosInmueble> RadicadosInmuebles { get; set; } = new List<RadicadosInmueble>();

    public virtual ICollection<RadicadosOtorgante> RadicadosOtorgantes { get; set; } = new List<RadicadosOtorgante>();
}
