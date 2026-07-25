using System;

namespace Formix.Domain.Dtos
{
    public class SysConfiguracionSistemaDto
    {
        public int ConfiguracionId { get; set; }
        public string Clave { get; set; }
        public string? Valor { get; set; }
        public string? Descripcion { get; set; }
        public bool? EsSensible { get; set; }
    }
}
