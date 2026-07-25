namespace Formix.Domain.Dtos;

public class SysUsuarioResponseDto
{
    public int UsuarioId { get; set; }
    public int TenantId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public DateTime? UltimoLogin { get; set; }
    public bool? Activo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public bool? Bloqueado { get; set; }
    public int? IntentosLogin { get; set; }
    public List<SysRoleDto> Roles { get; set; } = new();
}
