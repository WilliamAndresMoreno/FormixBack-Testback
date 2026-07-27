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
    [RequirePermission(PermissionCodes.CrearPago, PermissionCodes.VerEscrituracion)]
    public class RadicadoPagosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenant;

        public RadicadoPagosController(AppDbContext context, IMapper mapper, ITenantContext tenant)
        {
            _context = context;
            _mapper = mapper;
            _tenant = tenant;
        }

        // GET: api/radicadopagos/5
        [HttpGet("{idRadicado}")]
        public async Task<ActionResult<RadicadosPagoDto>> GetRadicadoPagos(int idRadicado)
        {
            try
            {
                var item = await _context.RadicadosPagos.FirstOrDefaultAsync(p => p.IdRadicado == idRadicado);

                if (item == null)
                {
                    return NotFound();
                }

                var itemDto = _mapper.Map<RadicadosPagoDto>(item);
                return Ok(itemDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo los pagos del radicado: {ex.Message}");
            }
        }

        // POST: api/radicadopagos
        [HttpPost]
        public async Task<ActionResult<RadicadosPagoDto>> PostRadicadoPagos(RadicadosPagoDto itemDto)
        {
            try
            {
                var entity = _mapper.Map<RadicadosPago>(itemDto);

                _context.RadicadosPagos.Add(entity);
                await _context.SaveChangesAsync();

                // Auto-crear / actualizar actos en base al pago registrado
                await ActualizarActosSegunPagoAsync(entity.IdRadicado, entity.ValorVenta, entity.ValorCredito);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<RadicadosPagoDto>(entity);

                return CreatedAtAction(nameof(GetRadicadoPagos), new { idRadicado = dto.IdRadicado }, dto);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear los pagos del radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear los pagos del radicado: {ex.Message}");
            }
        }

        // PUT: api/radicadopagos/5
        [HttpPut("{idRadicado}")]
        public async Task<IActionResult> PutRadicadoPagos(int idRadicado, RadicadosPagoDto item)
        {
            if (idRadicado != item.IdRadicado)
            {
                return BadRequest();
            }

            var entity = await _context.RadicadosPagos.FirstOrDefaultAsync(p => p.IdRadicado == idRadicado);
            RadicadosPago activeEntity;
            if (entity == null)
            {
                // Si no existe, lo creamos
                activeEntity = _mapper.Map<RadicadosPago>(item);
                _context.RadicadosPagos.Add(activeEntity);
            }
            else
            {
                // Actualizamos los valores
                _mapper.Map(item, entity);
                // Aseguramos que el PK no se sobreescriba por un 0 que venga del frontend
                _context.Entry(entity).Property(x => x.IdRadicadoPagos).IsModified = false;
                activeEntity = entity;
            }

            try
            {
                await _context.SaveChangesAsync();

                // Auto-crear / actualizar actos en base al pago registrado
                await ActualizarActosSegunPagoAsync(idRadicado, activeEntity.ValorVenta, activeEntity.ValorCredito);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RadicadoPagosExists(idRadicado))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict("Conflicto de concurrencia al actualizar los pagos del radicado");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando los pagos del radicado: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/radicadopagos/5
        [HttpDelete("{idRadicado}")]
        public async Task<IActionResult> DeleteRadicadoPagos(int idRadicado)
        {
            try
            {
                var item = await _context.RadicadosPagos.FirstOrDefaultAsync(p => p.IdRadicado == idRadicado);
                if (item == null)
                {
                    return NotFound();
                }

                _context.RadicadosPagos.Remove(item);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar los pagos del radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar los pagos del radicado: {ex.Message}");
            }
        }

        private async Task ActualizarActosSegunPagoAsync(int idRadicado, decimal? valorVenta, decimal? valorCredito)
        {
            // 1. Manejo del Acto de Venta
            var radicadosActoVenta = await _context.RadicadosActos
                .Include(ra => ra.IdActoNavigation)
                .FirstOrDefaultAsync(ra => ra.IdRadicado == idRadicado && 
                    (ra.IdActoNavigation.Nombre.ToLower().Contains("venta") || 
                     (ra.IdActoNavigation.Abreviatura != null && ra.IdActoNavigation.Abreviatura.ToLower().Contains("venta"))));

            if (valorVenta.HasValue && valorVenta.Value > 0)
            {
                if (radicadosActoVenta != null)
                {
                    radicadosActoVenta.Cuantia = valorVenta.Value;
                    _context.Entry(radicadosActoVenta).State = EntityState.Modified;
                }
                else
                {
                    var actoVenta = await _context.Actos
                        .FirstOrDefaultAsync(a => a.Nombre.ToLower().Contains("venta") || 
                            (a.Abreviatura != null && a.Abreviatura.ToLower().Contains("venta")));

                    if (actoVenta != null)
                    {
                        var nuevoRadicadoActo = new RadicadosActo
                        {
                            IdRadicado = idRadicado,
                            IdActo = actoVenta.IdActo,
                            Cuantia = valorVenta.Value
                        };
                        _context.RadicadosActos.Add(nuevoRadicadoActo);
                    }
                }
            }
            else if (radicadosActoVenta != null)
            {
                _context.RadicadosActos.Remove(radicadosActoVenta);
            }

            // 2. Manejo del Acto de Hipoteca
            var radicadosActoHipoteca = await _context.RadicadosActos
                .Include(ra => ra.IdActoNavigation)
                .FirstOrDefaultAsync(ra => ra.IdRadicado == idRadicado && 
                    (ra.IdActoNavigation.Nombre.ToLower().Contains("hipoteca") || 
                     (ra.IdActoNavigation.Abreviatura != null && ra.IdActoNavigation.Abreviatura.ToLower().Contains("hipoteca"))));

            if (valorCredito.HasValue && valorCredito.Value > 0)
            {
                if (radicadosActoHipoteca != null)
                {
                    radicadosActoHipoteca.Cuantia = valorCredito.Value;
                    _context.Entry(radicadosActoHipoteca).State = EntityState.Modified;
                }
                else
                {
                    var actoHipoteca = await _context.Actos
                        .FirstOrDefaultAsync(a => a.Nombre.ToLower().Contains("hipoteca") || 
                            (a.Abreviatura != null && a.Abreviatura.ToLower().Contains("hipoteca")));

                    if (actoHipoteca != null)
                    {
                        var nuevoRadicadoActo = new RadicadosActo
                        {
                            IdRadicado = idRadicado,
                            IdActo = actoHipoteca.IdActo,
                            Cuantia = valorCredito.Value
                        };
                        _context.RadicadosActos.Add(nuevoRadicadoActo);
                    }
                }
            }
            else if (radicadosActoHipoteca != null)
            {
                _context.RadicadosActos.Remove(radicadosActoHipoteca);
            }
        }

        private bool RadicadoPagosExists(int id)
        {
            return _context.RadicadosPagos.Any(e => e.IdRadicado == id);
        }
    }
}
