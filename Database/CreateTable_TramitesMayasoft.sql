-- Tabla ancha donde se guarda toda la información de cada trámite extraído de MayasoftAPI
CREATE TABLE dbo.TramitesMayasoft (
    Id                          INT IDENTITY(1,1) PRIMARY KEY,
    IdSalesforce                NVARCHAR(50)   NOT NULL,   -- id del trámite (oportunidad) en Salesforce
    IdBoletinVenta              NVARCHAR(50)   NULL,
    TipoVenta                   NVARCHAR(100)  NULL,
    Ciudad                      NVARCHAR(100)  NULL,
    MontoCredito                DECIMAL(18,2)  NULL,
    MontoSubsidio               DECIMAL(18,2)  NULL,
    SumaCierreFinanciero        DECIMAL(18,2)  NULL,

    -- Apoderado
    ApoderadoNombre             NVARCHAR(200)  NULL,
    ApoderadoCorreo             NVARCHAR(200)  NULL,
    ApoderadoTipoIdentificacion NVARCHAR(50)   NULL,
    ApoderadoNumeroIdentificacion NVARCHAR(50) NULL,
    ApoderadoCiudadExpedicion   NVARCHAR(100)  NULL,
    ApoderadoEstadoCivil        NVARCHAR(50)   NULL,

    -- Comprador titular
    CompradorId                 NVARCHAR(50)   NULL,
    CompradorNombre             NVARCHAR(200)  NULL,
    CompradorCorreo             NVARCHAR(200)  NULL,
    CompradorCelular            NVARCHAR(30)   NULL,
    CompradorTipoIdentificacion NVARCHAR(50)   NULL,
    CompradorNumeroIdentificacion NVARCHAR(50) NULL,
    CompradorCiudadExpedicion   NVARCHAR(100)  NULL,
    CompradorEstadoCivil        NVARCHAR(50)   NULL,

    -- Proyecto
    ProyectoId                  NVARCHAR(50)   NULL,
    ProyectoNombre              NVARCHAR(200)  NULL,
    ProyectoIdSinco             NVARCHAR(50)   NULL,

    -- Macroproyecto
    MacroproyectoId             NVARCHAR(50)   NULL,
    MacroproyectoNombre         NVARCHAR(200)  NULL,
    MacroproyectoEmpresa        NVARCHAR(200)  NULL,
    MacroproyectoEmailNotaria   NVARCHAR(200)  NULL,

    -- Caso trámite
    CasoId                       NVARCHAR(50)   NULL,
    CasoNumero                   NVARCHAR(50)   NULL,
    CasoAsunto                   NVARCHAR(300)  NULL,
    CasoEstado                   NVARCHAR(100)  NULL,
    CasoNotaria                  NVARCHAR(200)  NULL,
    EntidadFinancieraId          NVARCHAR(50)   NULL,
    EntidadFinancieraNombre      NVARCHAR(200)  NULL,
    EjecutarValidacionDocumentos BIT            NULL,
    NegocioListoParaEscriturar   BIT            NULL,
    AplicaSubsidioMinVivienda    BIT            NULL,
    FechaProyectadaProgramadaFinal DATE         NULL,
    ValidacionDocumentosFechaCheck DATE         NULL,
    LinkCarpetaCliente           NVARCHAR(500)  NULL,

    -- Recursos propios
    RecursosPropiosValorPagado   DECIMAL(18,2)  NULL,

    -- Crédito terceros / leasing (mutuamente excluyentes)
    CreditoLeasingTipo           NVARCHAR(30)   NULL, -- 'credito_terceros' | 'leasing_habitacional'
    CreditoLeasingMonto          DECIMAL(18,2)  NULL,
    CreditoLeasingEntidad        NVARCHAR(200)  NULL,
    CreditoLeasingFechaAprobacion DATE          NULL,

    -- Subsidio caja de compensación
    SubsidioCajaMonto            DECIMAL(18,2)  NULL,
    SubsidioCajaFechaAprobacion  DATE           NULL,
    SubsidioCajaEntidad          NVARCHAR(200)  NULL,

    -- Subsidio hábitat
    SubsidioHabitatTipo          NVARCHAR(50)   NULL,
    SubsidioHabitatMonto         DECIMAL(18,2)  NULL,
    SubsidioHabitatNoResolucion  NVARCHAR(50)   NULL,
    SubsidioHabitatFechaResolucion DATE         NULL,
    SubsidioHabitatEntidad       NVARCHAR(200)  NULL,

    -- Unidad principal (primer producto: apto/casa/local)
    UnidadPrincipalId            NVARCHAR(50)   NULL,
    UnidadPrincipalNombre        NVARCHAR(200)  NULL,
    UnidadPrincipalMatricula     NVARCHAR(100)  NULL,
    UnidadPrincipalPrecio        DECIMAL(18,2)  NULL,

    -- Arreglos guardados como JSON (varios registros posibles por trámite)
    CompradoresAlternosJson      NVARCHAR(MAX)  NULL,
    PlanDePagoJson               NVARCHAR(MAX)  NULL,
    CesantiasJson                NVARCHAR(MAX)  NULL,
    GarajesJson                  NVARCHAR(MAX)  NULL,
    DepositosJson                NVARCHAR(MAX)  NULL,
    DepositosUtilesJson          NVARCHAR(MAX)  NULL,

    -- Auditoría de la extracción
    FechaExtraccion              DATETIME       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_TramitesMayasoft_IdSalesforce UNIQUE (IdSalesforce)
);
