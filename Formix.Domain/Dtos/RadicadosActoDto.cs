using System;

namespace Formix.Domain.Dtos
{
    public class RadicadosActoDto
    {
        public int IdRadicado { get; set; }
        public int IdActo { get; set; }
        public decimal? Cuantia { get; set; }
        public decimal? Avaluo { get; set; }
        public DateOnly? FechaAdquisicion { get; set; }
    }
}
