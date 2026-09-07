namespace Formix.Infrastructure.ExternalServices;

// Representa la sección "MayasoftApi" del appsettings.json, para acceder a la configuración con IOptions<T>
public class MayasoftApiOptions
{
    public string TokenUrl { get; set; } = null!;      // URL para pedir el token OAuth2
    public string BaseUrl { get; set; } = null!;        // URL base de la instancia Salesforce
    public string Endpoint { get; set; } = null!;       // Ruta del endpoint de trámites
    public string ClientId { get; set; } = null!;       // Consumer Key configurado en appsettings
    public string ClientSecret { get; set; } = null!;   // Consumer Secret configurado en appsettings
    public int SyncIntervalHours { get; set; } = 24;    // Horas entre cada sincronización automática

    // Valores que la API de Cusezar no envía y que Formix necesita para crear el radicado
    public int TenantId { get; set; } = 1;                          // Tenant de Cusezar en Formix
    public int UsuarioSistemaId { get; set; } = 3;                  // Usuario "sistema" dueño de los radicados automáticos (3 = Admin)
    public int IdEstadoRadicadoInicial { get; set; } = 1;           // Estado inicial en TiposEstadoRadicado
    public int IdActoCompraventa { get; set; } = 33;                // IdActo de compraventa en la tabla Actos (33 = CVBIEI)
    public string ActoCodigoCompraventa { get; set; } = "CVBIEI";   // Código del acto para RadicadosOtorgantesTipos
    public int IdTipoOtorganteComprador { get; set; } = 1;          // IdTipoOtorgante del comprador titular (1 = Comprador Principal)
    public int IdTipoOtorganteCompradorAlterno { get; set; } = 2;   // IdTipoOtorgante de los compradores alternos (2 = Comprador Alterno)
    public bool CrearProyectoSiNoExiste { get; set; } = true;       // Crear el proyecto en Formix si no se encuentra por nombre
    public string MunicipioCodigoDaneDefault { get; set; } = "11001"; // Municipio para proyectos creados automáticamente
}
