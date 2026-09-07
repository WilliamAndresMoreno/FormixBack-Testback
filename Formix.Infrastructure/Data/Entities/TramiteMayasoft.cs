namespace Formix.Infrastructure.Data.Entities;

// Entidad plana (tabla ancha) donde se guarda toda la información extraída de un trámite de MayasoftAPI
public class TramiteMayasoft
{
    public int Id { get; set; }
    public string IdSalesforce { get; set; } = null!;
    public string? IdBoletinVenta { get; set; }
    public string? TipoVenta { get; set; }
    public string? Ciudad { get; set; }
    public decimal? MontoCredito { get; set; }
    public decimal? MontoSubsidio { get; set; }
    public decimal? SumaCierreFinanciero { get; set; }

    public string? ApoderadoNombre { get; set; }
    public string? ApoderadoCorreo { get; set; }
    public string? ApoderadoTipoIdentificacion { get; set; }
    public string? ApoderadoNumeroIdentificacion { get; set; }
    public string? ApoderadoCiudadExpedicion { get; set; }
    public string? ApoderadoEstadoCivil { get; set; }

    public string? CompradorId { get; set; }
    public string? CompradorNombre { get; set; }
    public string? CompradorCorreo { get; set; }
    public string? CompradorCelular { get; set; }
    public string? CompradorTipoIdentificacion { get; set; }
    public string? CompradorNumeroIdentificacion { get; set; }
    public string? CompradorCiudadExpedicion { get; set; }
    public string? CompradorEstadoCivil { get; set; }

    public string? ProyectoId { get; set; }
    public string? ProyectoNombre { get; set; }
    public string? ProyectoIdSinco { get; set; }

    public string? MacroproyectoId { get; set; }
    public string? MacroproyectoNombre { get; set; }
    public string? MacroproyectoEmpresa { get; set; }
    public string? MacroproyectoEmailNotaria { get; set; }

    public string? CasoId { get; set; }
    public string? CasoNumero { get; set; }
    public string? CasoAsunto { get; set; }
    public string? CasoEstado { get; set; }
    public string? CasoNotaria { get; set; }
    public string? EntidadFinancieraId { get; set; }
    public string? EntidadFinancieraNombre { get; set; }
    public bool? EjecutarValidacionDocumentos { get; set; }
    public bool? NegocioListoParaEscriturar { get; set; }
    public bool? AplicaSubsidioMinVivienda { get; set; }
    public DateTime? FechaProyectadaProgramadaFinal { get; set; }
    public DateTime? ValidacionDocumentosFechaCheck { get; set; }
    public string? LinkCarpetaCliente { get; set; }

    public decimal? RecursosPropiosValorPagado { get; set; }

    public string? CreditoLeasingTipo { get; set; }
    public decimal? CreditoLeasingMonto { get; set; }
    public string? CreditoLeasingEntidad { get; set; }
    public DateTime? CreditoLeasingFechaAprobacion { get; set; }

    public decimal? SubsidioCajaMonto { get; set; }
    public DateTime? SubsidioCajaFechaAprobacion { get; set; }
    public string? SubsidioCajaEntidad { get; set; }

    public string? SubsidioHabitatTipo { get; set; }
    public decimal? SubsidioHabitatMonto { get; set; }
    public string? SubsidioHabitatNoResolucion { get; set; }
    public DateTime? SubsidioHabitatFechaResolucion { get; set; }
    public string? SubsidioHabitatEntidad { get; set; }

    public string? UnidadPrincipalId { get; set; }
    public string? UnidadPrincipalNombre { get; set; }
    public string? UnidadPrincipalMatricula { get; set; }
    public decimal? UnidadPrincipalPrecio { get; set; }

    // Arreglos del JSON original guardados como texto JSON (la tabla es ancha, no relacional)
    public string? CompradoresAlternosJson { get; set; }
    public string? PlanDePagoJson { get; set; }
    public string? CesantiasJson { get; set; }
    public string? GarajesJson { get; set; }
    public string? DepositosJson { get; set; }
    public string? DepositosUtilesJson { get; set; }

    public DateTime FechaExtraccion { get; set; } = DateTime.Now;

    // Control de idempotencia: radicado de Formix creado a partir de este trámite (null = aún no distribuido a las tablas normalizadas)
    public int? IdRadicado { get; set; }
    public DateTime? FechaUltimaSincronizacion { get; set; } // Última vez que este trámite se procesó (insert o update)
}
