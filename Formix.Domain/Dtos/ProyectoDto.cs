using System;

namespace Formix.Domain.Dtos
{
    public class ProyectoDto
    {
        public int ProyectoId { get; set; }
        public int TenantId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string MunicipioCodigoDane { get; set; }
        public string Direccion { get; set; }
        public string MatriculaInmobiliaria { get; set; }
        public string? LinderoGeneral { get; set; }
    }
}
