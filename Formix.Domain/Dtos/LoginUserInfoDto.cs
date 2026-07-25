namespace Formix.Domain.Dtos;

public class LoginUserInfoDto
{
    public int UsuarioId { get; set; }
    public int TenantId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public bool EsAdministrador { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permisos { get; set; } = new();
}
