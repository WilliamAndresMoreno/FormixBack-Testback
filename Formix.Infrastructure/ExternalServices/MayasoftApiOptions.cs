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
}
