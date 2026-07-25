namespace Formix.Domain.Dtos.Mayasoft;

// DTO que representa la respuesta del endpoint de autenticación OAuth2 de Salesforce
public class MayasoftAuthResponseDto
{
    public string access_token { get; set; } = null!; // Token de acceso que se debe enviar en cada llamada a MayasoftAPI
    public string token_type { get; set; } = null!;    // Tipo de token, normalmente "Bearer"
    public string instance_url { get; set; } = null!;  // URL de la instancia de Salesforce que emitió el token
}