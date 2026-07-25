using AutoMapper;
using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authorization;
using Formix.Api.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InmuebleOtorganteController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public InmuebleOtorganteController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ✅ Obtener todos los otorgantes por inmueble
    [HttpGet("{inmuebleId}")]
    [RequirePermission(PermissionCodes.VerInmuebles)]
    public async Task<ActionResult<IEnumerable<ListaOtorganteDto>>> GetByInmueble(int inmuebleId)
    {
        try
        {
            var otorgantes = await _context.ListaOtorgantes
                .Where(it => it.IdInmueble == inmuebleId)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<ListaOtorganteDto>>(otorgantes));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener otorgantes del inmueble.", detail = ex.Message });
        }
    }

    // ✅ Crear otorgante
    [HttpPost]
    [RequirePermission(PermissionCodes.EditarInmuebles)]
    public async Task<ActionResult> Post(InmublesTerceroDto dto)
    {
        try
        {
            var entity = _mapper.Map<InmublesTercero>(dto);
            _context.InmublesTerceros.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<InmublesTerceroDto>(entity));
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, new { message = "Error al crear la relación inmueble↔tercero.", detail = dbEx.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error inesperado al crear la relación inmueble↔tercero.", detail = ex.Message });
        }
    }

    // ✅ Actualizar
    // Ahora usamos clave compuesta (IdInmueble, IdTercero)
    [HttpPut("{idInmueble}/{idTercero}")]
    [RequirePermission(PermissionCodes.EditarInmuebles)]
    public async Task<ActionResult> Put(int idInmueble, int idTercero, InmublesTerceroDto dto)
    {
        try
        {
            //if (idInmueble != dto.IdInmueble || idTercero != dto.IdTercero) return BadRequest();
            var entity = await _context.InmublesTerceros.Where(i => i.IdInmueble == idInmueble && i.IdTercero == idTercero).FirstOrDefaultAsync();
            if (entity == null) return NotFound();

            entity.IdTipoOtorgante = dto.IdTipoOtorgante;

            _context.InmublesTerceros.Update(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return StatusCode(409, new { message = "Conflicto al actualizar la relación (concurrencia).", detail = ex.Message });
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, new { message = "Error al actualizar la relación inmueble↔tercero.", detail = dbEx.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error inesperado al actualizar la relación inmueble↔tercero.", detail = ex.Message });
        }
    }

    // ✅ Eliminar
    [HttpDelete("{idInmueble}/{idTercero}")]
    [RequirePermission(PermissionCodes.EditarInmuebles)]
    public async Task<ActionResult> Delete(int idInmueble, int idTercero)
    {
        try
        {
            var entity = await _context.InmublesTerceros.FindAsync(idInmueble, idTercero);
            if (entity == null) return NotFound();

            _context.InmublesTerceros.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, new { message = "Error al eliminar la relación inmueble↔tercero.", detail = dbEx.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error inesperado al eliminar la relación inmueble↔tercero.", detail = ex.Message });
        }
    }
}
