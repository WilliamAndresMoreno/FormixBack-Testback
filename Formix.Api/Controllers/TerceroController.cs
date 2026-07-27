using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Formix.Domain.Dtos;
using Microsoft.AspNetCore.Authorization;
using Formix.Api.Authorization;

namespace Formix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TerceroController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenant;

        public TerceroController(AppDbContext context, IMapper mapper, ITenantContext tenant)
        {
            _context = context;
            _mapper = mapper;
            _tenant = tenant;
        }

        // GET: api/tercero
        [HttpGet]
        [RequirePermission(PermissionCodes.VerTerceros, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<TerceroDto>>> GetTerceros()
        {
            try
            {
                var tenantId = _tenant.TenantId;
                if (_tenant.TenantId <= 0)
                {
                    return Unauthorized("Tenant no definido");
                }

                var lista = await _context.ListaTerceros.Where(p => p.TenantId == tenantId)
                    //.Include(t => t.TipoDocumento)
                    .OrderBy(t => t.NombreCompleto)
                    .ToListAsync();

                var listaDto = _mapper.Map<List<TerceroDto>>(lista);
                return Ok(listaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo terceros: {ex.Message}");
            }
        }

        // GET: api/tercero/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerTerceros, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<TerceroDto>> GetTercero(int id)
        {
            try
            {
                var tercero = await _context.Terceros
                    //.Include(t => t.TipoDocumento)
                    .FirstOrDefaultAsync(t => t.IdTercero == id);

                if (tercero == null)
                {
                    return NotFound();
                }

                var terceroDto = _mapper.Map<TerceroDto>(tercero);
                return Ok(terceroDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el tercero: {ex.Message}");
            }
        }

        // POST: api/tercero
        [HttpPost]
        [RequirePermission(PermissionCodes.CrearTerceros, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<TerceroDto>> PostTercero([FromBody] TerceroDto terceroDto)
        {
            try
            {
                if (terceroDto == null)
                {
                    return BadRequest("El cuerpo de la solicitud es requerido");
                }

                var tenantId = _tenant.TenantId;
                if (_tenant.TenantId <= 0)
                {
                    return Unauthorized("Tenant no definido");
                }
                terceroDto.TenantId = _tenant.TenantId;
                terceroDto.NombreCompleto = string.IsNullOrWhiteSpace(terceroDto.NombreCompleto)
                    ? ($"{terceroDto.Nombre ?? string.Empty} {terceroDto.Apellido ?? string.Empty}").Trim()
                    : terceroDto.NombreCompleto;

                //var entity = new Tercero
                //{
                //    Nombre = terceroDto.Nombre,
                //    Apellido = terceroDto.Apellido,
                //    IdTipoDocumento = terceroDto.IdTipoDocumento,
                //    Documento = terceroDto.Documento,
                //    Correo = terceroDto.Correo,
                //    Celular = terceroDto.Celular
                //};
                var entity = _mapper.Map<Tercero>(terceroDto);
                _context.Terceros.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<TerceroDto>(entity);

                return CreatedAtAction(nameof(GetTercero), new { id = dto.IdTercero }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el tercero: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear el tercero: {ex.Message}");
            }
        }

        // PUT: api/tercero/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.EditarTerceros, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> PutTercero(int id, [FromBody] TerceroDto terceroDto)
        {
            var tenantId = _tenant.TenantId;
            if (tenantId <= 0)
            {
                return Unauthorized("Tenant no definido");
            }
            terceroDto.TenantId = tenantId;
            if (terceroDto == null)
            {
                return BadRequest("El cuerpo de la solicitud es requerido");
            }

            // Permitir que el id viaje solo en la ruta
            if (terceroDto.IdTercero == 0)
            {
                terceroDto.IdTercero = id;
            }

            if (id != terceroDto.IdTercero)
            {
                return BadRequest("El id de la ruta no coincide con el del cuerpo");
            }

            var entity = await _context.Terceros.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            terceroDto.NombreCompleto = string.IsNullOrWhiteSpace(terceroDto.NombreCompleto)
                ? ($"{terceroDto.Nombre ?? string.Empty} {terceroDto.Apellido ?? string.Empty}").Trim()
                : terceroDto.NombreCompleto;

            // Solo actualizar propiedades que existen en el DTO
            //entity.Nombre = terceroDto.Nombre;
            //entity.Apellido = terceroDto.Apellido;
            //entity.IdTipoDocumento = terceroDto.IdTipoDocumento;
            //entity.Documento = terceroDto.Documento;
            //entity.Correo = terceroDto.Correo;
            //entity.Celular = terceroDto.Celular;
            _mapper.Map(terceroDto, entity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TerceroExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict("Conflicto de concurrencia al actualizar el tercero");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el tercero: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/tercero/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.EliminarTerceros, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> DeleteTercero(int id)
        {
            try
            {
                var tercero = await _context.Terceros.FindAsync(id);
                if (tercero == null)
                {
                    return NotFound();
                }

                _context.Terceros.Remove(tercero);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar el tercero: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar el tercero: {ex.Message}");
            }
        }

        private bool TerceroExists(int id)
        {
            return _context.Terceros.Any(e => e.IdTercero == id);
        }

        [HttpGet("buscar")]
        [RequirePermission(PermissionCodes.VerTerceros, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> Buscar(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest("Debe enviar término de búsqueda");

                var tenantId = _tenant.TenantId;
                if (tenantId <= 0)
                {
                    return Unauthorized("Tenant no definido");
                }

                var result = await _context.ListaTerceros
                    .Where(t => t.TenantId == tenantId && ((t.NombreCompleto != null && t.NombreCompleto.Contains(term)) || (t.NumeroDocumento != null && t.NumeroDocumento.Contains(term))))
                    .ToListAsync();
                return Ok(_mapper.Map<IEnumerable<ListaOtorganteDto>>(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en la búsqueda de otorgantes: {ex.Message}");
            }
        }

    }
}