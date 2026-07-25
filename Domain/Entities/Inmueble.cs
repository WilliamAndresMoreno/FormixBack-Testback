using System;
using System.Collections.Generic;

namespace Formix.Infrastructure;

public partial class Inmueble
{
    public int InmuebleId { get; set; }

    public int TenantId { get; set; }

    public int ProyectoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? NombreCtl { get; set; }

    public string? NombreRph { get; set; }

    public int TipoInmuebleId { get; set; }

    public string? Unidad { get; set; }

    public string? Numero { get; set; }

    public string? MatriculaInmobiliaria { get; set; }

    public string? CedulaCatastral { get; set; }

    public string? ChipCatastral { get; set; }

    public string? LinderoEspecial { get; set; }

    public decimal? Coeficiente { get; set; }

    public string? Direccion { get; set; }

    public decimal? ValorInmueble { get; set; }

    public string? SubsidioEntidad { get; set; }

    public decimal? SubsidioValor { get; set; }

    public DateOnly? SubsidioFchAsigna { get; set; }

    public DateOnly? SubsidioFchAjusteAsigna { get; set; }

    public DateOnly? SubsidioFchCartaProrroga { get; set; }

    public string? CreditoEntidad { get; set; }

    public decimal? CreditoValor { get; set; }

    public string? CesantiasEntidad { get; set; }

    public decimal? CesantiasValor { get; set; }

    public string? AhorroEntidad { get; set; }

    public decimal? AhorroValor { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Proyecto Proyecto { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual TipoInmueble TipoInmueble { get; set; } = null!;
}
