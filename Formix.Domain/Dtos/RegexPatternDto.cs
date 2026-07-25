using System;

namespace Formix.Domain.Dtos
{
    public class RegexPatternDto
    {
        public int Id { get; set; }
        public string? Tipo { get; set; }
        public string? Patron { get; set; }
        public bool? EsNegativo { get; set; }
        public int? Prioridad { get; set; }
        public bool? Activo { get; set; }
    }
}
