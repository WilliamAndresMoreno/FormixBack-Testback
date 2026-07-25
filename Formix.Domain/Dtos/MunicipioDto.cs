using System;

namespace Formix.Domain.Dtos
{
    public class MunicipioDto
    {
        public string CodigoDane { get; set; }
        public string NombreMunicipio { get; set; }
        public string CodigoDepartamento { get; set; }
        public string? TipoMunicipio { get; set; }
    }
}
