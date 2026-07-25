namespace Formix.Domain.Dtos;

public class SysRoleDetailDto : SysRoleDto
{
    public List<SysPermisoDto> Permisos { get; set; } = new();
}
