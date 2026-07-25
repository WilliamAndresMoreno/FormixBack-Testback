using System;

namespace Formix.Domain.Dtos
{
    public class ListaRadicadosInmuebleDto
    {
        public int IdRadicado { get; set; }
        public int TenantId { get; set; }
        public int ProyectoId { get; set; }
        public int? PlantillaId { get; set; }
        public int UsuarioId { get; set; }
        public string? Consecutivo { get; set; }
        public DateTime FechaRadicado { get; set; }
        public int? Orden { get; set; }
        public int InmuebleId { get; set; }
        public string Nombre { get; set; }
        public string? NombreCtl { get; set; }
        public string? NombreRph { get; set; }
        public string? Unidad { get; set; }
        public string? Numero { get; set; }
        public string? MatriculaInmobiliaria { get; set; }
        public string? CedulaCatastral { get; set; }
        public string? ChipCatastral { get; set; }
        public string? LinderoEspecial { get; set; }
        public decimal? Coeficiente { get; set; }
        public string? Direccion { get; set; }
        public decimal? ValorInmueble { get; set; }
        public string? SubsidioEntidad { get; set; }
        public decimal? SubsidioValor { get; set; }
        public DateOnly? SubsidioFchAsigna { get; set; }
        public DateOnly? SubsidioFchAjusteAsigna { get; set; }
        public DateOnly? SubsidioFchCartaProrroga { get; set; }
        public string? CreditoEntidad { get; set; }
        public decimal? CreditoValor { get; set; }
        public string? CesantiasEntidad { get; set; }
        public decimal? CesantiasValor { get; set; }
        public string? AhorroEntidad { get; set; }
        public decimal? AhorroValor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public int? TipoInmuebleId { get; set; }
        public string? TipoInmuebleNombre { get; set; }
    }
}
