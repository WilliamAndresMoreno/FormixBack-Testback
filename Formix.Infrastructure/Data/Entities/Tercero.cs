using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class Tercero
{
    public int IdTercero { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public int? IdTipoDocumento { get; set; }

    public string? Documento { get; set; }

    public string? Correo { get; set; }

    public string? Celular { get; set; }

    public int? IdEstadoCivil { get; set; }

    public string? LugarExpedicionMunicipioCodigoDane { get; set; }

    public string? Direccion { get; set; }

    public int TenantId { get; set; }

    public virtual TiposDocumento? IdTipoDocumentoNavigation { get; set; }

    public virtual ICollection<InmublesTercero> InmublesTerceros { get; set; } = new List<InmublesTercero>();

    public virtual ICollection<RadicadosOtorgante> RadicadosOtorgantes { get; set; } = new List<RadicadosOtorgante>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<TiposInmuebleHomologación> TiposInmuebleHomologacións { get; set; } = new List<TiposInmuebleHomologación>();
}
