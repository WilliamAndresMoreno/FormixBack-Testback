using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class VDatosMinutum
{
    public int InmuebleId { get; set; }

    public int TenantId { get; set; }

    public int ProyectoId { get; set; }

    public string? NombreCtl { get; set; }

    public string? NombreRph { get; set; }

    public string? InmuebleMasivoMatriculaMasivo { get; set; }

    public string? InmuebleCedulaCatastral { get; set; }

    public string? InmuebleChipCatastral { get; set; }

    public string? InmuebleDireccion { get; set; }

    public string? InmuebleValorVenta { get; set; }

    public string? InmuebleValorVentaLetras { get; set; }

    public string? PagoSubsidioEntidad { get; set; }

    public string? PagoSubsidioValor { get; set; }

    public string? PagoSubsidioValorLetras { get; set; }

    public DateOnly? PagoSubsidioFchAsigna { get; set; }

    public string? PagoSubsidioFchAsignaLiteral { get; set; }

    public DateOnly? PagoSubsidioFchAjusteAsigna { get; set; }

    public DateOnly? PagoSubsidioFchCartaProrroga { get; set; }

    public string? PagoCreditoEntidad { get; set; }

    public string? PagoCreditoValor { get; set; }

    public string? PagoCreditoValorLetras { get; set; }

    public string? PagoCesantiasEntidad { get; set; }

    public string? PagoCesantiasValor { get; set; }

    public string? PagoCesantiasValorLetras { get; set; }

    public string? PagoAhorroEntidad { get; set; }

    public string? PagoAhorroValor { get; set; }

    public string? PagoAhorroValorLetras { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public string? InmuebleValorHipoteca { get; set; }

    public string? InmuebleValorHipotecaLetras { get; set; }

    public int NumeroRadicado { get; set; }
}
