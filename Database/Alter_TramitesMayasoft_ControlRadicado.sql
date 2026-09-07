-- Convierte TramitesMayasoft en tabla de control: guarda el radicado de Formix generado por cada trámite
-- para que la sincronización sea idempotente (no duplique radicados en ejecuciones posteriores).
ALTER TABLE dbo.TramitesMayasoft
    ADD IdRadicado               INT      NULL,   -- FK al radicado creado a partir de este trámite
        FechaUltimaSincronizacion DATETIME NULL;  -- Última vez que el trámite se insertó o actualizó
GO

ALTER TABLE dbo.TramitesMayasoft
    ADD CONSTRAINT FK_TramitesMayasoft_Radicados
        FOREIGN KEY (IdRadicado) REFERENCES dbo.Radicados (IdRadicado);
GO

-- Evita que el mismo trámite de Salesforce quede registrado dos veces
CREATE UNIQUE INDEX UX_TramitesMayasoft_IdSalesforce ON dbo.TramitesMayasoft (IdSalesforce);
GO
