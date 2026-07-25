namespace Formix.Domain.Dtos.Mayasoft;

// Representa un trámite completo devuelto por MayasoftAPI (GET /services/apexrest/MayasoftAPI/v2/tramites)
public class TramiteMayasoftResponseDto
{
    public string Id { get; set; } = null!;
    public string? IdBoletinDeVenta { get; set; }
    public string? TipoDeVenta { get; set; }
    public string? Ciudad { get; set; }
    public decimal? MontoDeCredito { get; set; }
    public decimal? MontoSubsidio { get; set; }
    public decimal? SumaCierreFinanciero { get; set; }

    public string? NombreApoderado { get; set; }
    public string? CorreoApoderado { get; set; }
    public string? TipoDeIdentificacionApoderado { get; set; }
    public string? NumeroDeIdentificacionApoderado { get; set; }
    public string? CiudadDeExpedicionApoderado { get; set; }
    public string? EstadoCivilApoderado { get; set; }

    public CompradorDto? CompradorTitular { get; set; }
    public List<CompradorAlternoDto>? CompradoresAlternos { get; set; }
    public ProyectoDto? Proyecto { get; set; }
    public MacroproyectoDto? Macroproyecto { get; set; }
    public CasoTramiteDto? CasoTramite { get; set; }
    public List<PlanDePagoDto>? PlanDePago { get; set; }
    public RecursosPropiosDto? RecursosPropios { get; set; }
    public SubsidiosDto? Subsidios { get; set; }
    public CreditoLeasingDto? CreditoTerceros { get; set; }
    public CreditoLeasingDto? LeasingHabitacional { get; set; }
    public List<CesantiaDto>? Cesantias { get; set; }
    public ProductoDto? UnidadPrincipal { get; set; }
    public List<ProductoDto>? Garajes { get; set; }
    public List<ProductoDto>? Depositos { get; set; }
    public List<ProductoDto>? DepositosUtiles { get; set; }
}

// Comprador titular del negocio (sección 3.3 de la documentación)
public class CompradorDto
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? Celular { get; set; }
    public string? TipoDeIdentificacion { get; set; }
    public string? NumeroDeIdentificacion { get; set; }
    public string? CiudadDeExpedicionDocumento { get; set; }
    public string? EstadoCivil { get; set; }
}

// Comprador alterno: misma estructura que el titular, con el id de la oportunidad relacionada (sección 3.4)
public class CompradorAlternoDto : CompradorDto
{
    public string? Oportunidad { get; set; }
}

// Proyecto inmobiliario asociado al trámite (sección 3.5)
public class ProyectoDto
{
    public string? Id { get; set; }
    public string? NombreDelProyecto { get; set; }
    public string? IdProyectoEnSinco { get; set; }
}

// Macroproyecto asociado (sección 3.6)
public class MacroproyectoDto
{
    public string? Id { get; set; }
    public string? NombreDelMacroproyecto { get; set; }
    public string? Empresa { get; set; }
    public string? EmailNotaria { get; set; }
}

// Entidad financiera referenciada dentro de caso_tramite
public class EntidadFinancieraDto
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
}

// Caso/trámite de escrituración propiamente dicho (sección 3.7)
public class CasoTramiteDto
{
    public string? Id { get; set; }
    public string? NumeroDeCaso { get; set; }
    public string? Asunto { get; set; }
    public string? Estado { get; set; }
    public string? Notaria { get; set; }
    public EntidadFinancieraDto? EntidadFinanciera { get; set; }
    public bool? EjecutarValidacionDeDocumentos { get; set; }
    public bool? NegocioListoParaEscriturar { get; set; }
    public bool? AplicaSubsidioMinVivienda { get; set; }
    public DateTime? FechaProyectadaProgramadaFinal { get; set; }
    public DateTime? ValidacionDeDocumentosFechaDeCheck { get; set; }
    public string? LinkCarpetaCliente { get; set; }
}

// Cada cuota del plan de pago (sección 3.8)
public class PlanDePagoDto
{
    public string? Id { get; set; }
    public string? OportunidadRelacionada { get; set; }
    public decimal? CuotaInicial { get; set; }
}

// Recursos propios pagados por el comprador (sección 3.9)
public class RecursosPropiosDto
{
    public decimal? ValorPactado { get; set; } // Corregido: la API real usa "valor_pactado", no "valor_pagado" como decía la documentación
}

// Contenedor de los subsidios activos del negocio (sección 3.10, aparece solo si hay subsidios)
public class SubsidiosDto
{
    public SubsidioCajaCompensacionDto? SubsidioCajaCompensacion { get; set; }
    public SubsidioHabitatDto? SubsidioHabitat { get; set; }
}

public class SubsidioCajaCompensacionDto
{
    public string? Tipo { get; set; }
    public decimal? Monto { get; set; }
    public DateTime? FechaAprobacionDeSubsidio { get; set; }
    public string? Entidad { get; set; }
}

public class SubsidioHabitatDto
{
    public string? Tipo { get; set; }
    public decimal? Monto { get; set; }
    public string? NoResolucion { get; set; }
    public DateTime? FechaResolucionHabitat { get; set; }
    public string? Entidad { get; set; }
}

// Crédito de terceros o leasing habitacional: son mutuamente excluyentes (sección 3.11)
public class CreditoLeasingDto
{
    public decimal? Monto { get; set; }
    public string? Entidad { get; set; }
    public DateTime? FechaDeAprobacionDeCredito { get; set; }
}

// Registro de cesantías usado como recurso de pago (sección 3.12)
public class CesantiaDto
{
    public decimal? Monto { get; set; }
    public string? Entidad { get; set; }
}

// Producto (unidad principal, garaje, depósito o depósito útil) (sección 3.13)
public class ProductoDto
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
    public string? Matricula { get; set; }
    public decimal? Precio { get; set; }
}