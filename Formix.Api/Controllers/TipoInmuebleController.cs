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
    public class TipoInmuebleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TipoInmuebleController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/tipoinmueble
        [HttpGet]
        [RequirePermission(PermissionCodes.VerCatalogos, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<TipoInmuebleDto>>> GetTiposInmueble()
        {
            try
            {
                var lista = await _context.TipoInmuebles
                    .Where(t => t.Activo)
                    .OrderBy(t => t.Nombre)
                    .ToListAsync();
                
                var listaDto = _mapper.Map<List<TipoInmuebleDto>>(lista);
                return Ok(listaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo tipos de inmueble: {ex.Message}");
            }
        }

        // GET: api/tipoinmueble/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerCatalogos, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<TipoInmuebleDto>> GetTipoInmueble(int id)
        {
            try
            {
                var tipoInmueble = await _context.TipoInmuebles.FindAsync(id);

                if (tipoInmueble == null)
                {
                    return NotFound();
                }

                var tipoInmuebleDto = _mapper.Map<TipoInmuebleDto>(tipoInmueble);
                return Ok(tipoInmuebleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el tipo de inmueble: {ex.Message}");
            }
        }

        // POST: api/tipoinmueble
        [HttpPost]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<ActionResult<TipoInmuebleDto>> PostTipoInmueble(TipoInmuebleDto tipoInmuebleDto)
        {
            try
            {
                var entity = _mapper.Map<TipoInmueble>(tipoInmuebleDto);
                entity.Activo = true; // Por defecto activo al crear

                _context.TipoInmuebles.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<TipoInmuebleDto>(entity);

                return CreatedAtAction(nameof(GetTipoInmueble), new { id = dto.TipoInmuebleId }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el tipo de inmueble: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear el tipo de inmueble: {ex.Message}");
            }
        }

        // PUT: api/tipoinmueble/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<IActionResult> PutTipoInmueble(int id, TipoInmuebleDto tipoInmuebleDto)
        {
            if (id != tipoInmuebleDto.TipoInmuebleId)
            {
                return BadRequest();
            }

            var entity = await _context.TipoInmuebles.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            // Actualizar solo los campos permitidos
            entity.Nombre = tipoInmuebleDto.Nombre;
            entity.Activo = tipoInmuebleDto.Activo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoInmuebleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict("Conflicto de concurrencia al actualizar el tipo de inmueble");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el tipo de inmueble: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/tipoinmueble/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<IActionResult> DeleteTipoInmueble(int id)
        {
            try
            {
                var tipoInmueble = await _context.TipoInmuebles.FindAsync(id);
                if (tipoInmueble == null)
                {
                    return NotFound();
                }

                // Soft delete - solo marcar como inactivo
                tipoInmueble.Activo = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar el tipo de inmueble: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar el tipo de inmueble: {ex.Message}");
            }
        }

        private bool TipoInmuebleExists(int id)
        {
            return _context.TipoInmuebles.Any(e => e.TipoInmuebleId == id);
        }
    }
}