using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure;
using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Formix.Api;
using Formix.Api.Authorization;

namespace FormixBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProyectoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenant;

        public ProyectoController(AppDbContext context, IMapper mapper, ITenantContext tenant)
        {
            _context = context;
            _mapper = mapper;
            _tenant = tenant;
        }

        // GET: api/proyecto
        [HttpGet]
        [RequirePermission(PermissionCodes.VerProyectos, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<ProyectoDto>>> GetProyectos()
        {
            try
            {
                var tenantId = _tenant.TenantId;
                if (_tenant.TenantId <= 0)
                {
                    return Unauthorized("Tenant no definido");
                }

                var lista = await _context.Proyectos.Where(p => p.TenantId == tenantId).ToListAsync();
                var listaDto = _mapper.Map<List<ProyectoDto>>(lista);
                return Ok(listaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo proyectos: {ex.Message}");
            }
        }

        // GET: api/proyecto/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerProyectos)]
        public async Task<ActionResult<ProyectoDto>> GetProyecto(int id)
        {
            try
            {
                var proyecto = await _context.Proyectos.FindAsync(id);

                if (proyecto == null)
                {
                    return NotFound();
                }

                var proyectoDto = _mapper.Map<ProyectoDto>(proyecto);
                return Ok(proyectoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el proyecto: {ex.Message}");
            }
        }

        // POST: api/proyecto
        [HttpPost]
        [RequirePermission(PermissionCodes.CrearProyectos)]
        public async Task<ActionResult<ProyectoDto>> PostProyecto(ProyectoDto proyecto)
        {
            try
            {
                // 1. Mapear DTO → Entity
                var entity = _mapper.Map<Proyecto>(proyecto);

                _context.Proyectos.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<ProyectoDto>(entity);

                return CreatedAtAction(nameof(GetProyecto), new { id = dto.ProyectoId }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el proyecto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear el proyecto: {ex.Message}");
            }
        }

        // PUT: api/proyecto/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.EditarProyectos)]
        public async Task<IActionResult> PutProyecto(int id, ProyectoDto proyecto)
        {
            if (id != proyecto.ProyectoId)
            {
                return BadRequest();
            }

            var entity = _mapper.Map<Proyecto>(proyecto);
            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProyectoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict("Conflicto de concurrencia al actualizar el proyecto");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el proyecto: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/proyecto/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.EliminarProyectos)]
        public async Task<IActionResult> DeleteProyecto(int id)
        {
            try
            {
                var proyecto = await _context.Proyectos.FindAsync(id);
                if (proyecto == null)
                {
                    return NotFound();
                }

                _context.Proyectos.Remove(proyecto);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar el proyecto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar el proyecto: {ex.Message}");
            }
        }

        private bool ProyectoExists(int id)
        {
            return _context.Proyectos.Any(e => e.ProyectoId == id);
        }
    }
}
