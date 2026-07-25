using System;

namespace Formix.Domain.Dtos
{
    public class TiposEstadoCivilDto
    {
        public int IdEstadoCivil { get; set; }
        public string EstadoCivil { get; set; }
        public string? Descripcion { get; set; }
        public string? Codigo { get; set; }
    }
}
