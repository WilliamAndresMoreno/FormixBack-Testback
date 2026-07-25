using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class VInmueblesMinutum
{
    public int InmuebleId { get; set; }

    public int TenantId { get; set; }

    public int ProyectoId { get; set; }

    public int IdRadicado { get; set; }

    public string InmuebleBlNombre { get; set; } = null!;

    public string? InmuebleBlNombreLetras { get; set; }

    public string? InmuebleBlMatricula { get; set; }

    public string? InmuebleBlLinderoEspecial { get; set; }

    public string? InmuebleBlValor { get; set; }

    public string? InmuebleBlValorLetras { get; set; }

    public decimal? InmuebleBlCoeficiente { get; set; }

    public string? InmuebleBlCoeficienteLetras { get; set; }

    public int? Orden { get; set; }

    public string? InmuebleBlUnidad { get; set; }

    public string? InmuebleBlNumero { get; set; }

    public int? TipoInmuebleId { get; set; }

    public string? InmuebleBlTipoInmueble { get; set; }
}
