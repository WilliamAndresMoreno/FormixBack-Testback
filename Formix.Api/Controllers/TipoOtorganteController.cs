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
    public class TipoOtorganteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TipoOtorganteController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/tipootorgante
        [HttpGet]
        [RequirePermission(PermissionCodes.VerCatalogos, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<TiposOtorganteDto>>> GetTiposOtorgante()
        {
            var lista = await _context.TiposOtorgantes
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            var listaDto = _mapper.Map<List<TiposOtorganteDto>>(lista);
            return Ok(listaDto);
        }

        // GET: api/tipootorgante/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerCatalogos, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<TiposOtorganteDto>> GetTipoOtorgante(int id)
        {
            var entity = await _context.TiposOtorgantes.FindAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<TiposOtorganteDto>(entity);
            return Ok(dto);
        }

        // POST: api/tipootorgante
        [HttpPost]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<ActionResult<TiposOtorganteDto>> PostTipoOtorgante(TiposOtorganteDto dto)
        {
            var entity = _mapper.Map<TiposOtorgante>(dto);

            _context.TiposOtorgantes.Add(entity);
            await _context.SaveChangesAsync();

            var createdDto = _mapper.Map<TiposOtorganteDto>(entity);
            return CreatedAtAction(nameof(GetTipoOtorgante), new { id = createdDto.IdTipoOtorgante }, createdDto);
        }

        // PUT: api/tipootorgante/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<IActionResult> PutTipoOtorgante(int id, TiposOtorganteDto dto)
        {
            if (id != dto.IdTipoOtorgante)
            {
                return BadRequest();
            }

            var entity = await _context.TiposOtorgantes.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            // Actualizar campos permitidos
            entity.Nombre = dto.Nombre;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TiposOtorgantes.AnyAsync(e => e.IdTipoOtorgante == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/tipootorgante/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.GestionarCatalogos)]
        public async Task<IActionResult> DeleteTipoOtorgante(int id)
        {
            var entity = await _context.TiposOtorgantes.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.TiposOtorgantes.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
