using System;

namespace Formix.Domain.Dtos
{
    public class MinVariableDto
    {
        public int VariableId { get; set; }
        public int CategoriaVariableId { get; set; }
        public string Variable { get; set; }
        public string? Codigo { get; set; }
        public bool Activo { get; set; }
    }
}
