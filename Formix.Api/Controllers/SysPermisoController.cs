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
    public class SysPermisoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public SysPermisoController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/syspermiso
        [HttpGet]
        [RequirePermission(PermissionCodes.VerPermisos)]
        public async Task<IActionResult> GetPermisos([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
        {
            try
            {
                var query = _context.SysPermisos.AsQueryable();

                if (page.HasValue && pageSize.HasValue)
                {
                    var total = await query.CountAsync();
                    var items = await query
                        .OrderByDescending(p => p.PermisoId)
                        .Skip((Math.Max(1, page.Value) - 1) * pageSize.Value)
                        .Take(pageSize.Value)
                        .ToListAsync();
                    
                    var itemsDto = _mapper.Map<List<SysPermisoDto>>(items);
                    return Ok(new { items = itemsDto, total = total });
                }
                else
                {
                    var lista = await query
                        .OrderBy(p => p.Nombre)
                        .ToListAsync();
                    
                    var listaDto = _mapper.Map<List<SysPermisoDto>>(lista);
                    return Ok(listaDto);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo permisos: {ex.Message}");
            }
        }

        // GET: api/syspermiso/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerPermisos)]
        public async Task<ActionResult<SysPermisoDto>> GetPermiso(int id)
        {
            try
            {
                var permiso = await _context.SysPermisos.FindAsync(id);

                if (permiso == null)
                {
                    return NotFound();
                }

                var permisoDto = _mapper.Map<SysPermisoDto>(permiso);
                return Ok(permisoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el permiso: {ex.Message}");
            }
        }

        // POST: api/syspermiso
        [HttpPost]
        [RequirePermission(PermissionCodes.CrearPermisos)]
        public async Task<ActionResult<SysPermisoDto>> PostPermiso(SysPermisoDto permisoDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permisoDto.Codigo))
                {
                    return BadRequest("El código de permiso es requerido");
                }

                permisoDto.Codigo = permisoDto.Codigo.Trim().ToUpper().Replace(" ", "_");

                if (await _context.SysPermisos.AnyAsync(p => p.Codigo == permisoDto.Codigo))
                {
                    return BadRequest("Ya existe un permiso con este código");
                }

                var entity = _mapper.Map<SysPermiso>(permisoDto);

                _context.SysPermisos.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<SysPermisoDto>(entity);

                return CreatedAtAction(nameof(GetPermiso), new { id = dto.PermisoId }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el permiso: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear el permiso: {ex.Message}");
            }
        }

        // PUT: api/syspermiso/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.EditarPermisos)]
        public async Task<IActionResult> PutPermiso(int id, SysPermisoDto permisoDto)
        {
            if (id != permisoDto.PermisoId)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(permisoDto.Codigo))
            {
                return BadRequest("El código de permiso es requerido");
            }

            permisoDto.Codigo = permisoDto.Codigo.Trim().ToUpper().Replace(" ", "_");

            if (await _context.SysPermisos.AnyAsync(p => p.Codigo == permisoDto.Codigo && p.PermisoId != id))
            {
                return BadRequest("Ya existe otro permiso con este código");
            }

            var entity = await _context.SysPermisos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            // Actualizar propiedades
            entity.Codigo = permisoDto.Codigo;
            entity.Nombre = permisoDto.Nombre;
            entity.Descripcion = permisoDto.Descripcion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermisoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict("Conflicto de concurrencia al actualizar el permiso");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el permiso: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/syspermiso/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.EliminarPermisos)]
        public async Task<IActionResult> DeletePermiso(int id)
        {
            try
            {
                var permiso = await _context.SysPermisos.FindAsync(id);
                if (permiso == null)
                    return NotFound();

                var links = await _context.SysRolPermisos.Where(rp => rp.PermisoId == id).ToListAsync();
                _context.SysRolPermisos.RemoveRange(links);
                _context.SysPermisos.Remove(permiso);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar el permiso: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar el permiso: {ex.Message}");
            }
        }

        private bool PermisoExists(int id)
        {
            return _context.SysPermisos.Any(e => e.PermisoId == id);
        }
    }
}