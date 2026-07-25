using System;

namespace Formix.Domain.Dtos
{
    public class DepartamentoDto
    {
        public string CodigoDane { get; set; }
        public string NombreDepartamento { get; set; }
        public string? NombreCorto { get; set; }
        public string? CodigoIso { get; set; }
    }
}
