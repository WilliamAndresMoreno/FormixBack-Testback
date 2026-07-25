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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ActoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ActoController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/acto
        [HttpGet]
        [RequirePermission(PermissionCodes.VerCatalogos)]
        public async Task<ActionResult<IEnumerable<ActoDto>>> GetActos()
        {
            try
            {
                var lista = await _context.Actos
                    .OrderBy(a => a.Nombre)
                    .ToListAsync();

                var listaDto = _mapper.Map<List<ActoDto>>(lista);
                return Ok(listaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo actos: {ex.Message}");
            }
        }

        // GET: api/acto/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerCatalogos)]
        public async Task<ActionResult<ActoDto>> GetActo(int id)
        {
            try
            {
                var acto = await _context.Actos.FindAsync(id);

                if (acto == null)
                    return NotFound("Acto no encontrado");

                var actoDto = _mapper.Map<ActoDto>(acto);
                return Ok(actoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el acto: {ex.Message}");
            }
        }

        // POST: api/acto
        [HttpPost]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<ActionResult<ActoDto>> PostActo(ActoDto actoDto)
        {
            try
            {
                var entity = _mapper.Map<Acto>(actoDto);

                _context.Actos.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<ActoDto>(entity);

                return CreatedAtAction(nameof(GetActo), new { id = dto.IdActo }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al guardar el acto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado: {ex.Message}");
            }
        }

        // PUT: api/acto/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<IActionResult> PutActo(int id, ActoDto actoDto)
        {
            if (id != actoDto.IdActo)
                return BadRequest("El ID no coincide");

            try
            {
                var entity = await _context.Actos.FindAsync(id);
                if (entity == null)
                    return NotFound("Acto no encontrado");

                entity.Nombre = actoDto.Nombre;
                entity.Abreviatura = actoDto.Abreviatura;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Conflicto de concurrencia al actualizar el acto");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el acto: {ex.Message}");
            }
        }

        // DELETE: api/acto/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<IActionResult> DeleteActo(int id)
        {
            try
            {
                var acto = await _context.Actos.FindAsync(id);
                if (acto == null)
                    return NotFound("Acto no encontrado");

                _context.Actos.Remove(acto);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar el acto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado: {ex.Message}");
            }
        }

        private bool ActoExists(int id)
        {
            return _context.Actos.Any(e => e.IdActo == id);
        }
    }
}
