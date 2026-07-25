using System;

namespace Formix.Domain.Dtos
{
    public class VInmueblesMinutumDto
    {
        public int InmuebleId { get; set; }
        public int TenantId { get; set; }
        public int ProyectoId { get; set; }
        public int IdRadicado { get; set; }
        public string InmuebleBlNombre { get; set; }
        public string? InmuebleBlNombreLetras { get; set; }
        public string? InmuebleBlMatricula { get; set; }
        public string? InmuebleBlLinderoEspecial { get; set; }
        public string? InmuebleBlValor { get; set; }
        public string? InmuebleBlValorLetras { get; set; }
        public string? InmuebleBlCoeficiente { get; set; }
        public string? InmuebleBlCoeficienteLetras { get; set; }
        public int? Orden { get; set; }
        public string? InmuebleBlUnidad { get; set; }
        public string? InmuebleBlNumero { get; set; }
        public int? TipoInmuebleId { get; set; }
        public string? InmuebleBlTipoInmueble { get; set; }
    }
}
