using AutoMapper;
using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Formix.Api;
using Formix.Api.Authorization;

namespace FormixBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RadicadoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly PdfEscrituracionService _pdf;
        private readonly ITenantContext _tenant;

        public RadicadoController(AppDbContext context, IMapper mapper, PdfEscrituracionService pdf, ITenantContext tenant)
        {
            _context = context;
            _mapper = mapper;
            _pdf = pdf;
            _tenant = tenant;
        }

        // GET: api/radicado
        [HttpGet]
        [RequirePermission(PermissionCodes.VerRadicados)]
        public async Task<ActionResult<IEnumerable<RadicadoDto>>> GetRadicados()
        {
            try
            {
                var tenantId = _tenant.TenantId;
                if (_tenant.TenantId <= 0)
                {
                    return Unauthorized("Tenant no definido");
                }

                var lista = await _context.ListaRadicados.Where(p => p.TenantId == tenantId)
                    .OrderByDescending(r => r.Consecutivo)
                    .ToListAsync();
                return Ok(_mapper.Map<List<RadicadoDto>>(lista));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: api/ordenescrituracion
        [HttpGet("ordenes_escrituracion")]
        [RequirePermission(PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<RadicadoDto>>> GetListaOrdenEscrituracion()
        {
            try
            {
                var tenantId = _tenant.TenantId;
                if (_tenant.TenantId <= 0)
                {
                    return Unauthorized("Tenant no definido");
                }

                var lista = await _context.ListaOrdenEscrituracions.Where(p => p.TenantId == tenantId)
                    .OrderByDescending(r => r.IdRadicado)
                    .ToListAsync();
                return Ok(_mapper.Map<List<RadicadoDto>>(lista));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<RadicadoDto>> GetRadicado(int id)
        {
            try
            {
                var entity = await _context.Radicados
                    .FirstOrDefaultAsync(r => r.IdRadicado == id);
                if (entity == null) return NotFound();
                return Ok(_mapper.Map<RadicadoDto>(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo el radicado: {ex.Message}");
            }
        }

        // GET: api/radicado/5/pdf
        [HttpGet("{id}/pdf")]
        [RequirePermission(PermissionCodes.VerRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> DownloadPdf(int id, [FromServices] Formix.Api.Services.PdfGeneratorService pdfService)
        {
            try
            {
                // Cargar datos
                var radicado = await _context.Radicados
                    .Include(r => r.Proyecto)
                    .Include(r => r.RadicadosOtorgantes)
                        .ThenInclude(ro => ro.IdTerceroNavigation)
                    .Include(r => r.RadicadosOtorgantes)
                        .ThenInclude(ro => ro.RadicadosOtorgantesTipos)
                            .ThenInclude(rot => rot.IdTipoOtorganteNavigation)
                    .Include(r => r.RadicadosInmuebles)
                        .ThenInclude(ri => ri.IdInmuebleNavigation)
                            .ThenInclude(i => i.TipoInmueble)
                    .Include(r => r.RadicadosPagos)
                        .ThenInclude(rp => rp.IdCajaCompensacionNavigation)
                    .FirstOrDefaultAsync(r => r.IdRadicado == id);

                if (radicado == null) return NotFound("Radicado no encontrado.");

                // Leer HTML
                var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "OrdenEscrituracionTemplate.html");
                var html = await System.IO.File.ReadAllTextAsync(templatePath);

                // Ocultar logo por defecto (si luego se quiere enviar por parámetro)
                html = html.Replace("{{LogoUrl}}", "");
                html = html.Replace("{{DisplayLogoImage}}", "none");
                html = html.Replace("{{DisplayLogo}}", "none");

                // Fechas
                html = html.Replace("{{FechaOE}}", (radicado.FechaOe ?? radicado.FechaRadicado)?.ToString("dd/MM/yyyy") ?? "");

                // Cargar catálogos para mapeos en memoria
                var municipioCodes = radicado.RadicadosOtorgantes
                    .Select(ro => ro.IdTerceroNavigation?.LugarExpedicionMunicipioCodigoDane)
                    .Where(code => !string.IsNullOrEmpty(code))
                    .Distinct()
                    .ToList();

                var municipiosMap = await _context.Municipios
                    .Include(m => m.CodigoDepartamentoNavigation)
                    .Where(m => municipioCodes.Contains(m.CodigoDane))
                    .ToDictionaryAsync(m => m.CodigoDane, m => m);

                var estadoCivilIds = radicado.RadicadosOtorgantes
                    .Select(ro => ro.IdTerceroNavigation?.IdEstadoCivil)
                    .Where(id => id.HasValue)
                    .Distinct()
                    .Select(id => id!.Value)
                    .ToList();

                var estadoCivilMap = await _context.TiposEstadoCivils
                    .Where(ec => estadoCivilIds.Contains(ec.IdEstadoCivil))
                    .ToDictionaryAsync(ec => ec.IdEstadoCivil, ec => string.IsNullOrWhiteSpace(ec.Descripcion) ? ec.EstadoCivil : $"{ec.EstadoCivil} ({ec.Descripcion})");

                // Otorgantes dinámicos
                var clientesHtml = new System.Text.StringBuilder();
                foreach (var ro in radicado.RadicadosOtorgantes)
                {
                    var c = ro.IdTerceroNavigation;
                    if (c == null) continue;

                    var tipoOtorganteRel = ro.RadicadosOtorgantesTipos.FirstOrDefault();
                    var tipoNombre = tipoOtorganteRel?.IdTipoOtorganteNavigation?.Nombre ?? "Cliente";
                    var porcentajeVal = tipoOtorganteRel?.Porcentaje;
                    var porcentajeStr = porcentajeVal.HasValue ? $"{porcentajeVal.Value:0.#}%" : "-";

                    var estadoCivilStr = "-";
                    if (c.IdEstadoCivil.HasValue && estadoCivilMap.TryGetValue(c.IdEstadoCivil.Value, out var ecNombre))
                    {
                        estadoCivilStr = ecNombre;
                    }

                    var expedicionStr = "-";
                    var deptoStr = "";
                    if (!string.IsNullOrEmpty(c.LugarExpedicionMunicipioCodigoDane) && 
                        municipiosMap.TryGetValue(c.LugarExpedicionMunicipioCodigoDane, out var munEntity))
                    {
                        expedicionStr = munEntity.NombreMunicipio;
                        deptoStr = $" ({munEntity.CodigoDepartamentoNavigation?.NombreDepartamento ?? ""})";
                    }

                    clientesHtml.Append($@"
                    <div class=""info-block"" style=""margin-bottom: 15px; border-bottom: 1px dashed #ccc; padding-bottom: 10px;"">
                      <div class=""info-row"">
                        <div class=""info-label"">{tipoNombre.ToUpper()}:</div>
                        {c.NombreCompleto} <span class=""info-label"" style=""margin-left: 15px"">Participación</span> {porcentajeStr}
                      </div>
                      <div class=""info-row"">
                        <div class=""info-label"">C.C. No.</div>
                        {c.Documento ?? "-"} <span class=""info-label"" style=""margin-left: 10px"">de</span> {expedicionStr}{deptoStr}
                      </div>
                      <div class=""info-row"">
                        <div class=""info-label"">ESTADO CIVIL:</div>
                        {estadoCivilStr}
                      </div>
                      <div class=""info-row"" style=""margin-top: 5px"">
                        <div class=""info-label"">Correo electrónico:</div>
                        {c.Correo ?? "-"} <span class=""info-label"" style=""margin-left: 20px"">Celular:</span> {c.Celular ?? "-"}
                      </div>
                    </div>");
                }
                if (clientesHtml.Length == 0)
                {
                    clientesHtml.Append(@"<div class=""info-row""><div class=""info-label"">CLIENTE:</div>-</div>");
                }

                html = html.Replace("{{ClientesHtml}}", clientesHtml.ToString());

                // Inmuebles dinámicos
                var inmueblesHtml = new System.Text.StringBuilder();
                foreach (var ri in radicado.RadicadosInmuebles)
                {
                    var inm = ri.IdInmuebleNavigation;
                    if (inm == null) continue;

                    var tipoInmueble = inm.TipoInmueble?.Nombre ?? "Inmueble";

                    inmueblesHtml.Append($@"
                    <div style=""margin-bottom: 10px; border-bottom: 1px dashed #eee; padding-bottom: 5px;"">
                      <div class=""info-row"">
                        <div class=""info-label"">{tipoInmueble.ToUpper()}:</div>
                        {inm.Nombre ?? "-"}
                      </div>
                      <div class=""info-row"">
                        <div class=""info-label"">MATRICULA:</div>
                        {inm.MatriculaInmobiliaria ?? "-"}
                      </div>
                    </div>");
                }
                if (inmueblesHtml.Length == 0)
                {
                    inmueblesHtml.Append(@"<div class=""info-row""><div class=""info-label"">INMUEBLE:</div>-</div>");
                }

                html = html.Replace("{{InmueblesHtml}}", inmueblesHtml.ToString());

                // Pagos
                var pago1 = radicado.RadicadosPagos.FirstOrDefault();
                var ci = new System.Globalization.CultureInfo("es-CO");

                Func<decimal?, string> formatMoney = (val) => {
                    return val.HasValue ? val.Value.ToString("C0", ci) : "$ 0";
                };

                html = html.Replace("{{ValorVenta}}", formatMoney(pago1?.ValorVenta));
                html = html.Replace("{{ValorEscritura}}", formatMoney(pago1?.ValorEscritura));
                html = html.Replace("{{CuotaInicial}}", formatMoney(pago1?.CuotaInicial));
                html = html.Replace("{{CajaCompensacion}}", !string.IsNullOrEmpty(pago1?.IdCajaCompensacionNavigation?.Nombre) ? pago1.IdCajaCompensacionNavigation.Nombre : "-");
                html = html.Replace("{{ValorSubsidioCaja}}", formatMoney(pago1?.ValorSubsidioCc));
                html = html.Replace("{{CajaHabitat}}", !string.IsNullOrEmpty(pago1?.SubsidioEntidad) ? pago1.SubsidioEntidad : "-");
                html = html.Replace("{{ValorSubsidioHabitat}}", formatMoney(pago1?.ValorSubsidioSh));
                html = html.Replace("{{ValorAnticipoSubsidio}}", formatMoney(pago1?.ValorAnticipoSubsudio));
                html = html.Replace("{{ValorSubsidioIndexacion}}", formatMoney(pago1?.ValorSubsidioIndexacion));
                html = html.Replace("{{BancoCredito}}", !string.IsNullOrEmpty(pago1?.CreditoEntidad) ? pago1.CreditoEntidad : "-");
                html = html.Replace("{{ValorCredito}}", formatMoney(pago1?.ValorCredito));

                // Footer
                html = html.Replace("{{Observaciones}}", "Y/O");
                html = html.Replace("{{Elaboro}}", "Sistema Formix");
                html = html.Replace("{{FechaElaboracion}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                html = html.Replace("{{TextoFooter}}", "");

                var pdfBytes = await pdfService.GenerarPdfDesdeHtmlAsync(html);
                return File(pdfBytes, "application/pdf", $"OrdenEscrituracion_{id}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar el PDF: {ex.Message}");
            }
        }

        private async Task<string> GenerateAutoConsecutivo(int tenantId)
        {
            var colombiaZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaZone);
            string prefix = localTime.ToString("yyyyMMdd");

            var todayConsecutivos = await _context.Radicados
                .Where(r => r.TenantId == tenantId && r.Consecutivo.StartsWith(prefix))
                .Select(r => r.Consecutivo)
                .ToListAsync();

            int nextNum = 1;
            if (todayConsecutivos.Any())
            {
                var numbers = todayConsecutivos
                    .Select(c => {
                        string lastPart = c.Substring(prefix.Length);
                        return int.TryParse(lastPart, out int num) ? num : 0;
                    })
                    .ToList();
                if (numbers.Any())
                {
                    nextNum = numbers.Max() + 1;
                }
            }
            return $"{prefix}{nextNum}";
        }

        // POST: api/radicado
        [HttpPost]
        [RequirePermission(PermissionCodes.CrearRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<RadicadoDto>> PostRadicado(RadicadoDto dto)
        {
            try
            {
                if (dto.IdRadicado == -1)
                {
                    dto.IdRadicado = null;
                }

                var tenantId = _tenant.TenantId;
                dto.TenantId = tenantId;

                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant != null && tenant.Consecutivo == true && dto.IdEstado == 1)
                {
                    dto.Consecutivo = await GenerateAutoConsecutivo(tenantId);
                }

                if (dto.IdEstado == 1)
                {
                    bool exists = await _context.Radicados.AnyAsync(r => r.ProyectoId == dto.ProyectoId && r.Consecutivo == dto.Consecutivo && r.IdEstado == 1);
                    if (exists) return BadRequest("Ya existe un Radicado con ese consecutivo para este proyecto.");
                }

                var entity = _mapper.Map<Radicado>(dto);
                _context.Radicados.Add(entity);
                await _context.SaveChangesAsync();
                var created = _mapper.Map<RadicadoDto>(entity);
                return Ok(created);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear el radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear el radicado: {ex.Message}");
            }
        }

        // PUT: api/radicado/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> PutRadicado(int id, RadicadoDto dto)
        {
            if (id != dto.IdRadicado) return BadRequest();

            var tenantId = _tenant.TenantId;
            dto.TenantId = tenantId;

            var entity = await _context.Radicados.FindAsync(id);
            if (entity == null) return NotFound();

            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null && tenant.Consecutivo == true && dto.IdEstado == 1 && string.IsNullOrEmpty(entity.Consecutivo))
            {
                dto.Consecutivo = await GenerateAutoConsecutivo(tenantId);
            }

            if (dto.IdEstado == 1)
            {
                bool exists = await _context.Radicados.AnyAsync(r => r.ProyectoId == dto.ProyectoId && r.Consecutivo == dto.Consecutivo && r.IdEstado == 1 && r.IdRadicado != id);
                if (exists) return BadRequest("Ya existe un Radicado con ese consecutivo para este proyecto.");
            }

            _mapper.Map(dto, entity);
            _context.Entry(entity).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Conflicto de concurrencia al actualizar el radicado");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando el radicado: {ex.Message}");
            }
            return Ok(_mapper.Map<RadicadoDto>(entity));
        }

        // DELETE: api/radicado/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.EliminarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> DeleteRadicado(int id)
        {
            try
            {
                var parameter = new SqlParameter("@RadicadoId", id);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC SP_DeleteRadicadoCascade @RadicadoId",
                    parameter
                );

                return Ok(new
                {
                    success = true,
                    message = "Radicado eliminado correctamente"
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar el radicado",
                    detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error inesperado al eliminar el radicado",
                    detail = ex.Message
                });
            }
        }

        // ===== Subrecursos: Actos =====
        // GET: api/radicado/{id}/actos
        [Authorize]
        [HttpGet("{id}/actos")]
        [RequirePermission(PermissionCodes.VerRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<RadicadosActoDto>>> GetActos(int id)
        {
            try
            {
                var radicado = await _context.Radicados.FindAsync(id);
                if (radicado == null) return NotFound();

                var items = await _context.RadicadosActos
                    .Where(x => x.IdRadicado == id)
                    .ToListAsync();
                return Ok(_mapper.Map<List<RadicadosActoDto>>(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo actos del radicado: {ex.Message}");
            }
        }

        // POST: api/radicado/{id}/actos
        [Authorize]
        [HttpPost("{id}/actos")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<RadicadosActoDto>> PostActo(int id, RadicadosActoDto dto)
        {
            if (id != dto.IdRadicado) return BadRequest();
            try
            {
                var radicado = await _context.Radicados.FindAsync(id);
                if (radicado == null) return NotFound();

                var entity = _mapper.Map<RadicadosActo>(dto);
                _context.RadicadosActos.Add(entity);
                await _context.SaveChangesAsync();
                return Ok(_mapper.Map<RadicadosActoDto>(entity));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear acto del radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear acto: {ex.Message}");
            }
        }

        // PUT: api/radicado/{id}/actos/{idActo}
        [Authorize]
        [HttpPut("{id}/actos/{idActo}")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> PutActo(int id, int idActo, RadicadosActoDto dto)
        {
            if (id != dto.IdRadicado || idActo != dto.IdActo) return BadRequest();
            try
            {
                var entity = await _context.RadicadosActos
                    .FirstOrDefaultAsync(x => x.IdRadicado == id && x.IdActo == idActo);
                if (entity == null) return NotFound();

                _mapper.Map(dto, entity);
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Conflicto de concurrencia al actualizar acto del radicado");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando acto del radicado: {ex.Message}");
            }
        }

        // DELETE: api/radicado/{id}/actos/{idActo}
        [Authorize]
        [HttpDelete("{id}/actos/{idActo}")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> DeleteActo(int id, int idActo)
        {
            try
            {
                var entity = await _context.RadicadosActos
                    .FirstOrDefaultAsync(x => x.IdRadicado == id && x.IdActo == idActo);
                if (entity == null) return NotFound();

                _context.RadicadosActos.Remove(entity);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar acto del radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar acto: {ex.Message}");
            }
        }

        // ===== Subrecursos: Otorgantes =====
        // GET: api/radicado/{id}/otorgantes
        [HttpGet("{id}/otorgantes")]
        [RequirePermission(PermissionCodes.VerRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<ListaOtorganteDto>>> GetOtorgantes(int id)
        {
            try
            {
                var items = await _context.ListaRadicadosOtorgantes
            .Where(x => x.IdRadicado == id)
            .ToListAsync();
                var lista = _mapper.Map<IEnumerable<ListaOtorganteDto>>(items);

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener otorgantes del inmueble.", detail = ex.Message });
            }
        }

        [HttpGet("{id}/RadicadoOtorgantes")]
        [RequirePermission(PermissionCodes.VerRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<RadicadosOtorganteDto>>> GetRadicadoOtorgantes(int id)
        {
            try
            {
                var radicado = await _context.Radicados.FindAsync(id);
                if (radicado == null) return NotFound();

                var items = await _context.RadicadosOtorgantes
                    .Where(x => x.IdRadicado == id)
                    .ToListAsync();
                return Ok(_mapper.Map<List<RadicadosOtorganteDto>>(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo otorgantes del radicado: {ex.Message}");
            }
        }

        // POST: api/radicado/{id}/otorgantes
        [HttpPost("{id}/otorgantes")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<RadicadosOtorganteDto>> PostOtorgante(int id, RadicadosOtorganteDto dto)
        {
            if (id != dto.IdRadicado) return BadRequest();
            try
            {
                var radicado = await _context.Radicados.FindAsync(id);
                if (radicado == null) return NotFound();

                var entity = await _context.RadicadosOtorgantes
                    .FirstOrDefaultAsync(x => x.IdRadicado == id && x.IdTercero == dto.IdTercero);

                if (entity == null)
                {
                    entity = _mapper.Map<RadicadosOtorgante>(dto);
                    _context.RadicadosOtorgantes.Add(entity);
                    await _context.SaveChangesAsync();
                }

                var existingType = await _context.RadicadosOtorgantesTipos
                    .FirstOrDefaultAsync(x => x.IdRadicadoOtorgante == entity.IdRadicadoOtorgante && x.IdTipoOtorgante == dto.IdTipoOtorgante);

                if (existingType == null)
                {
                    _context.RadicadosOtorgantesTipos.Add(new RadicadosOtorgantesTipo()
                    {
                        IdRadicadoOtorgante = entity.IdRadicadoOtorgante,
                        IdTipoOtorgante = dto.IdTipoOtorgante
                    });
                    await _context.SaveChangesAsync();
                }

                return Ok(_mapper.Map<RadicadosOtorganteDto>(entity));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear otorgante del radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear otorgante: {ex.Message}");
            }
        }

        // PUT: api/radicado/{id}/radicadootorgantes
        [HttpPut("{id}/radicadootorgantes")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> PutOtorgante(int id, RadicadosOtorganteDto dto)
        {
            if (id != dto.IdRadicado || dto.IdTercero <= 0) return BadRequest("Parámetros inválidos: id radicado o id tercero");
            try
            {
                var entity = await _context.RadicadosOtorgantes
                    .FirstOrDefaultAsync(x => x.IdRadicado == id && x.IdTercero == dto.IdTercero);
                if (entity == null) return NotFound();

                var query = _context.RadicadosOtorgantesTipos
                    .Where(x => x.IdRadicadoOtorgante == entity.IdRadicadoOtorgante);

                RadicadosOtorgantesTipo entity1 = null;
                if (dto.IdTipoOtorganteOriginal.HasValue && dto.IdTipoOtorganteOriginal > 0)
                {
                    entity1 = await query.FirstOrDefaultAsync(x => x.IdTipoOtorgante == dto.IdTipoOtorganteOriginal.Value);
                }
                else
                {
                    entity1 = await query.FirstOrDefaultAsync();
                }

                if (entity1 == null)
                {
                    _context.RadicadosOtorgantesTipos.Add(new RadicadosOtorgantesTipo()
                    {
                        IdRadicadoOtorgante = entity.IdRadicadoOtorgante,
                        IdTipoOtorgante = dto.IdTipoOtorgante
                    });
                }
                else
                {
                    entity1.IdTipoOtorgante = dto.IdTipoOtorgante;
                    _context.Entry(entity1).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
                return Ok(_mapper.Map<RadicadosOtorganteDto>(entity));
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Conflicto de concurrencia al actualizar otorgante del radicado");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando otorgante del radicado: {ex.Message}");
            }
        }

        // DELETE: api/radicado/{id}/otorgantes/{idTercero}
        [HttpDelete("{id}/otorgantes/{idTercero}")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> DeleteOtorgante(int id, int idTercero, [FromQuery] int? idTipoOtorgante = null)
        {
            try
            {
                var entity = await _context.RadicadosOtorgantes
                    .FirstOrDefaultAsync(x => x.IdRadicado == id && x.IdTercero == idTercero);
                if (entity == null) return NotFound();

                if (idTipoOtorgante.HasValue && idTipoOtorgante > 0)
                {
                    var typeEntity = await _context.RadicadosOtorgantesTipos
                        .FirstOrDefaultAsync(x => x.IdRadicadoOtorgante == entity.IdRadicadoOtorgante && x.IdTipoOtorgante == idTipoOtorgante.Value);
                    if (typeEntity != null)
                    {
                        _context.RadicadosOtorgantesTipos.Remove(typeEntity);
                        await _context.SaveChangesAsync();
                    }

                    bool remains = await _context.RadicadosOtorgantesTipos.AnyAsync(x => x.IdRadicadoOtorgante == entity.IdRadicadoOtorgante);
                    if (!remains)
                    {
                        _context.RadicadosOtorgantes.Remove(entity);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    _context.RadicadosOtorgantes.Remove(entity);
                    await _context.SaveChangesAsync();
                }
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar otorgante del radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar otorgante: {ex.Message}");
            }
        }

        // ===== Subrecursos: Inmuebles (asociaciones) =====
        // GET: api/radicado/{id}/inmuebles
        [HttpGet("{id}/inmuebles")]
        [RequirePermission(PermissionCodes.VerRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<ActionResult<IEnumerable<InmuebleDto>>> GetInmuebles(int id)
        {
            try
            {
                var radicadoInmuebles = await _context.RadicadosInmuebles
                    .Include(ri => ri.IdInmuebleNavigation)
                    .ThenInclude(i => i.TipoInmueble)
                    .Where(ri => ri.IdRadicado == id)
                    .ToListAsync();

                if (radicadoInmuebles == null) return NotFound();

                var lista = radicadoInmuebles.Select(ri => {
                    var dto = _mapper.Map<InmuebleDto>(ri.IdInmuebleNavigation);
                    dto.Orden = ri.Orden;
                    return dto;
                }).ToList();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo inmuebles del radicado: {ex.Message}");
            }
        }

        // POST: api/radicado/{id}/inmuebles/{inmuebleId}
        [HttpPost("{id}/inmuebles/{inmuebleId}")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> AddInmueble(int id, int inmuebleId)
        {
            try
            {
                var radicado = await _context.Radicados
                    .Include(r => r.RadicadosInmuebles)
                    .FirstOrDefaultAsync(r => r.IdRadicado == id);
                if (radicado == null) return NotFound();

                var inmueble = await _context.Inmuebles.FindAsync(inmuebleId);
                if (inmueble == null) return NotFound();

                if (!radicado.RadicadosInmuebles.Any(i => i.IdInmueble == inmuebleId))
                {
                    radicado.RadicadosInmuebles.Add(new RadicadosInmueble()
                    {
                        IdRadicado = id,
                        IdInmueble = inmuebleId,
                    });
                    await _context.SaveChangesAsync();
                }
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al asociar inmueble al radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al asociar inmueble: {ex.Message}");
            }
        }

        // PUT: api/radicado/{id}/radicadoinmuebles
        [HttpPut("{id}/radicadoinmuebles")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> PutInmuebles(int id, RadicadosInmuebleDto dto)
        {
            // Validación correcta: si no coincide el id del radicado o el tercero no es válido
            if (dto.IdRadicado <= 0 || dto.IdInmueble <= 0) return BadRequest("Parámetros inválidos: id radicado o id inmueble");
            try
            {
                var entity = await _context.RadicadosInmuebles
                    .FirstOrDefaultAsync(x => x.IdRadicado == dto.IdRadicado && x.IdInmueble == dto.IdInmueble);
                if (entity == null) return NotFound();

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_ActualizarRadicadoInmuebles @IdRadicado, @IdInmueble, @Orden",
                    new SqlParameter("@IdRadicado", dto.IdRadicado),
                    new SqlParameter("@IdInmueble", dto.IdInmueble),
                    new SqlParameter("@Orden", dto.Orden ?? (object)DBNull.Value)
                );

                return Ok(_mapper.Map<RadicadosInmuebleDto>(entity));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict("Conflicto de concurrencia al actualizar inmueble del radicado");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando inmueble del radicado: {ex.Message}");
            }
        }

        // DELETE: api/radicado/{id}/inmuebles/{inmuebleId}
        [HttpDelete("{id}/inmuebles/{inmuebleId}")]
        [RequirePermission(PermissionCodes.EditarRadicados, PermissionCodes.VerEscrituracion)]
        public async Task<IActionResult> RemoveInmueble(int id, int inmuebleId)
        {
            try
            {
                var radicado = await _context.Radicados
                    .Include(r => r.RadicadosInmuebles)
                    .FirstOrDefaultAsync(r => r.IdRadicado == id);
                if (radicado == null) return NotFound();

                var existing = radicado.RadicadosInmuebles.FirstOrDefault(i => i.IdInmueble == inmuebleId);
                if (existing == null) return NotFound();

                radicado.RadicadosInmuebles.Remove(existing);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al quitar inmueble del radicado: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al quitar inmueble: {ex.Message}");
            }
        }

        [HttpPost("cargar-pdf")]
        [RequirePermission(PermissionCodes.CargarPdfRadicado)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CargarPdf([FromForm] UploadPdfRequest request)
        {
            try
            {
                var tenantId = _tenant.TenantId;
                if (_tenant.TenantId <= 0)
                {
                    return Unauthorized("Tenant no definido");
                }

                var file = request.File;
                var idRadicado = request.IdRadicado;

                if (file == null || file.Length == 0)
                    return BadRequest("Archivo no válido");

                // Validar radicado existente
                var radicado = await _context.Radicados.FindAsync(idRadicado);
                if (radicado == null) return NotFound($"Radicado {idRadicado} no existe.");

                var ruta = Path.Combine(Path.GetTempPath(), file.FileName);

                using (var stream = new FileStream(ruta, FileMode.Create))
                    await file.CopyToAsync(stream);

                var (otorgantes, actos, inmuebles) = await _pdf.ExtraerAsync(ruta, radicado);

                // Limpiar archivo temporal
                System.IO.File.Delete(ruta);

                // Persistir en BD vía Stored Procedure con TVP (Table-Valued Parameters)
                try
                {
                    await GuardarDatosExtraidosConSp(tenantId, idRadicado, otorgantes, actos, inmuebles);
                }
                catch (Exception spEx)
                {
                    // Si falla la persistencia, seguimos devolviendo el resultado del parseo
                    // y reportamos el error de BD en el payload para diagnóstico.
                    return Ok(new
                    {
                        idRadicado,
                        datos = new { actos, otorgantes, inmuebles },
                        mensaje = "PDF procesado, pero ocurrió un error al persistir en BD",
                        errorPersistencia = spEx.Message
                    });
                }

                return Ok(new
                {
                    idRadicado,
                    datos = new
                    {
                        actos,
                        otorgantes,
                        inmuebles
                    },
                    mensaje = "PDF procesado correctamente"
                });
            }
            catch (Exception ex)
            {
                //return StatusCode(500, new
                //{
                //    mensaje = ex.Message,
                //    error = ex.Message
                //});
                // Si falla la persistencia, seguimos devolviendo el resultado del parseo
                // y reportamos el error de BD en el payload para diagnóstico.
                return Ok(new
                {
                    mensaje = "Se presento un error al procesar el PDF.",
                    errorPersistencia = ex.Message
                });
            }
        }

        // Helpers: ejecución de SP con TVPs
        private async Task GuardarDatosExtraidosConSp(int tenantId, int idRadicado,
            List<Otorgante> otorgantes,
            List<ActoCuantia> actos,
            List<InmuebleSabana> inmuebles)
        {
            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.sp_Radicado_PersistirSabana"; // SUPUESTO: nombre del SP
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // Parámetro escalar
            var pId = new Microsoft.Data.SqlClient.SqlParameter("@IdRadicado", System.Data.SqlDbType.Int)
            {
                Value = idRadicado
            };
            cmd.Parameters.Add(pId);

            // TVP: Otorgantes
            var valueOtorgantes = ToDataTableOtorgantes(otorgantes, tenantId);
            var tvpOtorgantes = new Microsoft.Data.SqlClient.SqlParameter("@Otorgantes", System.Data.SqlDbType.Structured)
            {
                TypeName = "dbo.tt_OtorganteSabana",
                Value = valueOtorgantes
            };
            cmd.Parameters.Add(tvpOtorgantes);

            var valueOtorgantesCalidades = ToDataTableOtorgantesCalidades(otorgantes);
            var tvpOtorgantesCalidades =
    new Microsoft.Data.SqlClient.SqlParameter("@OtorgantesCalidades", SqlDbType.Structured)
    {
        TypeName = "dbo.tt_OtorganteCalidadSabana",
        Value = valueOtorgantesCalidades
    };

            cmd.Parameters.Add(tvpOtorgantesCalidades);


            // TVP: Actos
            var tvpActos = new Microsoft.Data.SqlClient.SqlParameter("@Actos", System.Data.SqlDbType.Structured)
            {
                TypeName = "dbo.tt_ActoSabana",
                Value = ToDataTableActos(actos)
            };
            cmd.Parameters.Add(tvpActos);

            // TVP: Inmuebles
            var tvpInmuebles = new Microsoft.Data.SqlClient.SqlParameter("@Inmuebles", System.Data.SqlDbType.Structured)
            {
                TypeName = "dbo.tt_InmuebleSabana",
                Value = ToDataTableInmuebles(inmuebles)
            };
            cmd.Parameters.Add(tvpInmuebles);

            // Ejecutar
            if (cmd is Microsoft.Data.SqlClient.SqlCommand sqlCmd)
            {
                await sqlCmd.ExecuteNonQueryAsync();
            }
            else
            {
                // Para otros proveedores, usar ExecuteNonQuery() básico
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Conversión a DataTable para TVPs
        private static System.Data.DataTable ToDataTableOtorgantes(List<Otorgante> items, int tenantId)
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("TenantId", typeof(int));
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Documento", typeof(string));
            dt.Columns.Add("TipoDocumento", typeof(string));
            dt.Columns.Add("EstadoCivil", typeof(string));
            dt.Columns.Add("Direccion", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Telefono", typeof(string));
            dt.Columns.Add("Calidad", typeof(string));
            dt.Columns.Add("Porcentaje", typeof(string));
            dt.Columns.Add("AnioAdquisicion", typeof(string));
            dt.Columns.Add("CasaHabitacion", typeof(string));
            foreach (var o in items)
            {
                // Normalizar documento
                var documento = Regex.Replace(
                    o.Documento ?? "",
                    @"[^0-9]",
                    ""
                );

                dt.Rows.Add(tenantId, o.Nombre, documento, o.TipoDocumento, o.EstadoCivil, o.Direccion, o.Email, o.Telefono,
                    null, null, null, null);
            }
            return dt;
        }

        private static DataTable ToDataTableOtorgantesCalidades(List<Otorgante> otorgantes)
        {
            var dt = new DataTable();

            dt.Columns.Add("Documento", typeof(string));
            dt.Columns.Add("Calidad", typeof(string));
            dt.Columns.Add("ActoCodigo", typeof(string));
            dt.Columns.Add("Porcentaje", typeof(decimal));
            dt.Columns.Add("AnioAdquisicion", typeof(int));
            dt.Columns.Add("CasaHabitacion", typeof(string));

            foreach (var o in otorgantes)
            {
                if (o.Calidades == null || o.Calidades.Count == 0)
                    continue;

                // Normalizar documento
                var documento = Regex.Replace(
                    o.Documento ?? "",
                    @"[^0-9]",
                    ""
                );

                foreach (var c in o.Calidades)
                {
                    var row = dt.NewRow();

                    row["Documento"] = documento;
                    row["Calidad"] = c.Calidad;
                    row["ActoCodigo"] = c.ActoCodigo;

                    row["Porcentaje"] =
                        decimal.TryParse(
                            c.Porcentaje?.Replace("%", ""),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var p
                        ) ? p : (object)DBNull.Value;

                    row["AnioAdquisicion"] =
                        int.TryParse(c.AnioAdquisicion, out var anio)
                            ? anio
                            : (object)DBNull.Value;

                    row["CasaHabitacion"] =
                        string.IsNullOrWhiteSpace(c.CasaHabitacion)
                            ? (object)DBNull.Value
                            : c.CasaHabitacion;

                    dt.Rows.Add(row);
                }
            }

            return dt;
        }


        private static System.Data.DataTable ToDataTableActos(List<ActoCuantia> items)
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Descripcion", typeof(string));
            dt.Columns.Add("Cuantia", typeof(string));
            dt.Columns.Add("Avaluo", typeof(string));
            dt.Columns.Add("AnioAdquisicion", typeof(string));
            foreach (var a in items)
            {
                dt.Rows.Add(a.Descripcion, a.Cuantia, a.Avaluo, a.AnioAdquisicion);
            }
            return dt;
        }

        private static System.Data.DataTable ToDataTableInmuebles(List<InmuebleSabana> items)
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("MatriculaInmobiliaria", typeof(string));
            dt.Columns.Add("Municipio", typeof(string));
            dt.Columns.Add("Ubicacion", typeof(string));
            dt.Columns.Add("CedulaCatastral", typeof(string));
            dt.Columns.Add("ValorBien", typeof(string));
            foreach (var i in items)
            {
                dt.Rows.Add(i.MatriculaInmobiliaria, i.Municipio, i.Ubicacion, i.CedulaCatastral, i.ValorBien);
            }
            return dt;
        }
    }
}
