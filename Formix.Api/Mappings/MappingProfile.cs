using AutoMapper;
using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Entities;

namespace Formix.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Proyecto -> ProyectoDto
            CreateMap<Proyecto, ProyectoDto>().ReverseMap();

            CreateMap<RadicadosPago, RadicadosPagoDto>().ReverseMap();
            CreateMap<RadicadosPagoDto, RadicadosPago>().ReverseMap();

            // Inmueble -> InmuebleDto
            CreateMap<Inmueble, InmuebleDto>()
                .ForMember(d => d.TipoInmuebleNombre, opt => opt.MapFrom(s => s.TipoInmueble != null ? s.TipoInmueble.Nombre : null));

            // InmuebleDto -> Inmueble
            // Importante: ignorar la navegación TipoInmueble para evitar inserciones
            // accidentales en la tabla TipoInmuebles cuando solo se envía el Id.
            CreateMap<InmuebleDto, Inmueble>()
                .ForMember(s => s.TipoInmueble, opt => opt.Ignore());

            CreateMap<InmuebleDto, ListaRadicadosInmueble>().ReverseMap();

            // TipoInmueble -> TipoInmuebleDto
            CreateMap<TipoInmueble, TipoInmuebleDto>().ReverseMap();

            CreateMap<RadicadosInmueble, InmuebleDto>().ReverseMap();

            CreateMap<RadicadosInmuebleDto, RadicadosInmueble>().ReverseMap();


            // Acto -> ActoDto
            CreateMap<Acto, ActoDto>().ReverseMap();

            // SysPermiso -> SysPermisoDto
            CreateMap<SysPermiso, SysPermisoDto>().ReverseMap();

            // SysRole -> SysRoleDto
            CreateMap<SysRole, SysRoleDto>().ReverseMap();
            CreateMap<SysRole, SysRoleDetailDto>();

            // SysUsuario -> DTOs (sin exponer hash en respuestas)
            CreateMap<SysUsuario, SysUsuarioDto>().ReverseMap();
            CreateMap<SysUsuario, SysUsuarioResponseDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.SysUsuarioRoles.Select(ur => ur.Rol)));

            // Tercero -> TerceroDto
            CreateMap<Tercero, TerceroDto>().ReverseMap();

            // TiposDocumento -> TiposDocumentoDto
            CreateMap<TiposDocumento, TiposDocumentoDto>().ReverseMap();

            // TiposEstadoCivil -> TiposEstadoCivilDto
            CreateMap<TiposEstadoCivil, TiposEstadoCivilDto>().ReverseMap();

            // TiposInmuebleHomologación -> TiposInmuebleHomologaciónDto
            CreateMap<TiposInmuebleHomologación, TiposInmuebleHomologaciónDto>().ReverseMap();

            // TiposOtorgante -> TiposOtorganteDto
            CreateMap<TiposOtorgante, TiposOtorganteDto>().ReverseMap();

            // Suscripcione -> SuscripcioneDto
            CreateMap<Suscripcione, SuscripcioneDto>().ReverseMap();

            // Plane -> PlaneDto
            CreateMap<Plane, PlaneDto>().ReverseMap();

            CreateMap<InmublesTerceroDto, InmublesTercero>().ReverseMap();
            CreateMap<ListaOtorganteDto, ListaOtorgante>().ReverseMap();
            CreateMap<ListaOtorganteDto, ListaRadicadosOtorgante>().ReverseMap();
            CreateMap<ListaOtorganteDto, ListaTercero>().ReverseMap();
            CreateMap<TerceroDto, ListaTercero>().ReverseMap();

            // Radicados -> DTOs
            CreateMap<ListaRadicado, RadicadoDto>().ReverseMap();
            CreateMap<ListaOrdenEscrituracion, RadicadoDto>().ReverseMap();
            CreateMap<ListaOrdenEscrituracion, ListaOrdenEscrituracionDto>().ReverseMap();
            CreateMap<ListaOrdenEscrituracionDto, ListaOrdenEscrituracion>().ReverseMap();
            CreateMap<ListaRadicado, ListaRadicadoDto>().ReverseMap();
            CreateMap<RadicadoDto, ListaRadicado>().ReverseMap();
            CreateMap<ListaRadicadoDto, ListaRadicado>().ReverseMap();
            CreateMap<Radicado, RadicadoDto>().ReverseMap();
            CreateMap<RadicadosActo, RadicadosActoDto>().ReverseMap();
            CreateMap<RadicadosOtorgante, RadicadosOtorganteDto>().ReverseMap();

            // Plantilla -> PlantillaDto
            CreateMap<ProyectoPlantilla, ProyectoPlantillaDto>().ReverseMap();

        }
    }
}
