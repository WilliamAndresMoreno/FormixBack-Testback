using System;
using System.Collections.Generic;

namespace Formix.Infrastructure.Data.Entities;

public partial class RadicadosPago
{
    public int IdRadicadoPagos { get; set; }

    public int IdRadicado { get; set; }

    public int? IdCajaCompensacion { get; set; }

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

    public decimal? ValorVenta { get; set; }

    public decimal? ValorEscritura { get; set; }

    public decimal? CuotaInicial { get; set; }

    public decimal? ValorSubsidioCc { get; set; }

    public decimal? ValorSubsidioSh { get; set; }

    public decimal? ValorAnticipoSubsudio { get; set; }

    public decimal? ValorSubsidioIndexacion { get; set; }

    public decimal? ValorCredito { get; set; }

    public virtual CajasCompensacion? IdCajaCompensacionNavigation { get; set; }

    public virtual Radicado IdRadicadoNavigation { get; set; } = null!;
}
