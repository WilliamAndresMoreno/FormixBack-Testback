using System.Text.Json;
using Formix.Domain.Dtos.Mayasoft;
using Formix.Infrastructure.Data.Entities;

namespace Formix.Infrastructure.ExternalServices;

// Convierte el DTO anidado que llega de MayasoftAPI a la entidad plana TramiteMayasoft (tabla ancha)
public static class MayasoftTramiteMapper
{
    // Crea una entidad nueva a partir del DTO recibido de la API
    public static TramiteMayasoft MapToEntity(TramiteMayasoftResponseDto dto)
    {
        return new TramiteMayasoft
        {
            IdSalesforce = dto.Id,
            IdBoletinVenta = dto.IdBoletinDeVenta,
            TipoVenta = dto.TipoDeVenta,
            Ciudad = dto.Ciudad,
            MontoCredito = dto.MontoDeCredito,
            MontoSubsidio = dto.MontoSubsidio,
            SumaCierreFinanciero = dto.SumaCierreFinanciero,

            ApoderadoNombre = dto.NombreApoderado,
            ApoderadoCorreo = dto.CorreoApoderado,
            ApoderadoTipoIdentificacion = dto.TipoDeIdentificacionApoderado,
            ApoderadoNumeroIdentificacion = dto.NumeroDeIdentificacionApoderado,
            ApoderadoCiudadExpedicion = dto.CiudadDeExpedicionApoderado,
            ApoderadoEstadoCivil = dto.EstadoCivilApoderado,

            CompradorId = dto.CompradorTitular?.Id,
            CompradorNombre = dto.CompradorTitular?.Nombre,
            CompradorCorreo = dto.CompradorTitular?.CorreoElectronico,
            CompradorCelular = dto.CompradorTitular?.Celular,
            CompradorTipoIdentificacion = dto.CompradorTitular?.TipoDeIdentificacion,
            CompradorNumeroIdentificacion = dto.CompradorTitular?.NumeroDeIdentificacion,
            CompradorCiudadExpedicion = dto.CompradorTitular?.CiudadDeExpedicionDocumento,
            CompradorEstadoCivil = dto.CompradorTitular?.EstadoCivil,

            ProyectoId = dto.Proyecto?.Id,
            ProyectoNombre = dto.Proyecto?.NombreDelProyecto,
            ProyectoIdSinco = dto.Proyecto?.IdProyectoEnSinco,

            MacroproyectoId = dto.Macroproyecto?.Id,
            MacroproyectoNombre = dto.Macroproyecto?.NombreDelMacroproyecto,
            MacroproyectoEmpresa = dto.Macroproyecto?.Empresa,
            MacroproyectoEmailNotaria = dto.Macroproyecto?.EmailNotaria,

            CasoId = dto.CasoTramite?.Id,
            CasoNumero = dto.CasoTramite?.NumeroDeCaso,
            CasoAsunto = dto.CasoTramite?.Asunto,
            CasoEstado = dto.CasoTramite?.Estado,
            CasoNotaria = dto.CasoTramite?.Notaria,
            EntidadFinancieraId = dto.CasoTramite?.EntidadFinanciera?.Id,
            EntidadFinancieraNombre = dto.CasoTramite?.EntidadFinanciera?.Nombre,
            EjecutarValidacionDocumentos = dto.CasoTramite?.EjecutarValidacionDeDocumentos,
            NegocioListoParaEscriturar = dto.CasoTramite?.NegocioListoParaEscriturar,
            AplicaSubsidioMinVivienda = dto.CasoTramite?.AplicaSubsidioMinVivienda,
            FechaProyectadaProgramadaFinal = dto.CasoTramite?.FechaProyectadaProgramadaFinal,
            ValidacionDocumentosFechaCheck = dto.CasoTramite?.ValidacionDeDocumentosFechaDeCheck,
            LinkCarpetaCliente = dto.CasoTramite?.LinkCarpetaCliente,

            RecursosPropiosValorPagado = dto.RecursosPropios?.ValorPactado, // Corregido: la API real usa "valor_pactado", no "valor_pagado"

            // Crédito de terceros y leasing habitacional son mutuamente excluyentes: guardamos el que venga con un campo "Tipo" que indica cuál es
            CreditoLeasingTipo = dto.CreditoTerceros != null ? "credito_terceros"
                               : dto.LeasingHabitacional != null ? "leasing_habitacional" : null,
            CreditoLeasingMonto = dto.CreditoTerceros?.Monto ?? dto.LeasingHabitacional?.Monto,
            CreditoLeasingEntidad = dto.CreditoTerceros?.Entidad ?? dto.LeasingHabitacional?.Entidad,
            CreditoLeasingFechaAprobacion = dto.CreditoTerceros?.FechaDeAprobacionDeCredito ?? dto.LeasingHabitacional?.FechaDeAprobacionDeCredito,

            SubsidioCajaMonto = dto.Subsidios?.SubsidioCajaCompensacion?.Monto,
            SubsidioCajaFechaAprobacion = dto.Subsidios?.SubsidioCajaCompensacion?.FechaAprobacionDeSubsidio,
            SubsidioCajaEntidad = dto.Subsidios?.SubsidioCajaCompensacion?.Entidad,

            SubsidioHabitatTipo = dto.Subsidios?.SubsidioHabitat?.Tipo,
            SubsidioHabitatMonto = dto.Subsidios?.SubsidioHabitat?.Monto,
            SubsidioHabitatNoResolucion = dto.Subsidios?.SubsidioHabitat?.NoResolucion,
            SubsidioHabitatFechaResolucion = dto.Subsidios?.SubsidioHabitat?.FechaResolucionHabitat,
            SubsidioHabitatEntidad = dto.Subsidios?.SubsidioHabitat?.Entidad,

            UnidadPrincipalId = dto.UnidadPrincipal?.Id,
            UnidadPrincipalNombre = dto.UnidadPrincipal?.Nombre,
            UnidadPrincipalMatricula = dto.UnidadPrincipal?.Matricula,
            UnidadPrincipalPrecio = dto.UnidadPrincipal?.Precio,

            // Los arreglos (comprador alternos, plan de pago, cesantías, garajes, depósitos) se guardan como texto JSON en la tabla ancha
            CompradoresAlternosJson = Serialize(dto.CompradoresAlternos),
            PlanDePagoJson = Serialize(dto.PlanDePago),
            CesantiasJson = Serialize(dto.Cesantias),
            GarajesJson = Serialize(dto.Garajes),
            DepositosJson = Serialize(dto.Depositos),
            DepositosUtilesJson = Serialize(dto.DepositosUtiles),

            FechaExtraccion = DateTime.Now
        };
    }

    // Actualiza una entidad ya existente en la base de datos con los datos nuevos que llegaron de la API (para el upsert)
    public static void UpdateEntity(TramiteMayasoft entity, TramiteMayasoftResponseDto dto)
    {
        var mapped = MapToEntity(dto);
        mapped.Id = entity.Id;
        mapped.IdSalesforce = entity.IdSalesforce;

        // Copia todas las propiedades del objeto recién mapeado hacia la entidad existente, excepto la llave primaria
        // y las columnas de control (IdRadicado, FechaUltimaSincronizacion), que no vienen del DTO y no deben perderse
        foreach (var prop in typeof(TramiteMayasoft).GetProperties())
        {
            if (prop.Name is nameof(TramiteMayasoft.Id)
                or nameof(TramiteMayasoft.IdRadicado)
                or nameof(TramiteMayasoft.FechaUltimaSincronizacion)) continue;
            prop.SetValue(entity, prop.GetValue(mapped));
        }
    }

    // Serializa un arreglo a texto JSON, o devuelve null si el arreglo viene vacío/nulo
    private static string? Serialize<T>(T? data) =>
        data is null ? null : JsonSerializer.Serialize(data);
}