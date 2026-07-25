using System;

namespace Formix.Domain.Dtos
{
    public class SysPermisoDto
    {
        public int PermisoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}
