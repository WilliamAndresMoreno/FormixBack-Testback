using System;

namespace Formix.Domain.Dtos
{
    public class RadicadosOtorgantesTipoDto
    {
        public int IdRadicadoOtorganteTipo { get; set; }
        public int IdRadicadoOtorgante { get; set; }
        public int IdTipoOtorgante { get; set; }
        public string? ActoCodigo { get; set; }
        public decimal? Porcentaje { get; set; }
        public int? AnioAdquisicion { get; set; }
        public string? CasaHabitacion { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
