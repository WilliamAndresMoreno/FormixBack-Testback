using System;

namespace Formix.Domain.Dtos
{
    public class ActoDto
    {
        public int IdActo { get; set; }
        public string Nombre { get; set; }
        public string? Abreviatura { get; set; }
        public int? Estado { get; set; }
    }
}
