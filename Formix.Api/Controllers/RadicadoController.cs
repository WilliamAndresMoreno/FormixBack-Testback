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
                    //.OrderByDescending(r => r.IdRadicado)
                    .ToListAsync();
                return Ok(_mapper.Map<List<RadicadoDto>>(lista));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: api/radicado/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerRadicados)]
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

        // POST: api/radicado
        [HttpPost]
        [RequirePermission(PermissionCodes.CrearRadicados)]
        public async Task<ActionResult<RadicadoDto>> PostRadicado(RadicadoDto dto)
        {
            try
            {
                if (dto.IdRadicado == -1)
                {
                    dto.IdRadicado = null;
                }
                var entity = _mapper.Map<Radicado>(dto);
                _context.Radicados.Add(entity);
                await _context.SaveChangesAsync();
                var created = _mapper.Map<RadicadoDto>(entity);
                return CreatedAtAction(nameof(GetRadicado), new { id = created.IdRadicado }, created);
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
        [RequirePermission(PermissionCodes.EditarRadicados)]
        public async Task<IActionResult> PutRadicado(int id, RadicadoDto dto)
        {
            if (id != dto.IdRadicado) return BadRequest();
            var entity = await _context.Radicados.FindAsync(id);
            if (entity == null) return NotFound();

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
            return NoContent();
        }

        // DELETE: api/radicado/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.EliminarRadicados)]
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
        [RequirePermission(PermissionCodes.VerRadicados)]
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
        [RequirePermission(PermissionCodes.EditarRadicados)]
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
        [RequirePermission(PermissionCodes.EditarRadicados)]
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
        [RequirePermission(PermissionCodes.EditarRadicados)]
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
        [RequirePermission(PermissionCodes.VerRadicados)]
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
        [RequirePermission(PermissionCodes.VerRadicados)]
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
        [RequirePermission(PermissionCodes.EditarRadicados)]
        public async Task<ActionResult<RadicadosOtorganteDto>> PostOtorgante(int id, RadicadosOtorganteDto dto)
        {
            if (id != dto.IdRadicado) return BadRequest();
            try
            {
                var radicado = await _context.Radicados.FindAsync(id);
                if (radicado == null) return NotFound();

                var entity = _mapper.Map<RadicadosOtorgante>(dto);
                _context.RadicadosOtorgantes.Add(entity);
                await _context.SaveChangesAsync();
                _context.RadicadosOtorgantesTipos.Add(new RadicadosOtorgantesTipo()
                {
                    IdRadicadoOtorgante = entity.IdRadicadoOtorgante,
                    IdTipoOtorgante = dto.IdTipoOtorgante
                });
                await _context.SaveChangesAsync();
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
        [RequirePermission(PermissionCodes.EditarRadicados)]
        public async Task<IActionResult> PutOtorgante(int id, RadicadosOtorganteDto dto)
        {
            // Validación correcta: si no coincide el id del radicado o el tercero no es válido
            if (id != dto.IdRadicado || dto.IdTercero <= 0) return BadRequest("Parámetros inválidos: id radicado o id tercero");
            try
            {
                var entity = await _context.RadicadosOtorgantes
                    .FirstOrDefaultAsync(x => x.IdRadicado == id && x.IdTercero == dto.IdTercero);
                if (entity == null) return NotFound();

                var entity1 = await _context.RadicadosOtorgantesTipos
                    .FirstOrDefaultAsync(x => x.IdRadicadoOtorgante == entity.IdRadicadoOtorgante);

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
                    // Actualizar únicamente el campo requerido
                    entity1.IdTipoOtorgante = dto.IdTipoOtorgante;
                    _context.Entry(entity).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
                return Ok(_mapper.Map<RadicadosOtorganteDto>(entity));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict("Conflicto de concurrencia al actualizar otorgante del radicado");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando otorgante del radicado: {ex.Message}");
            }
        }

        // DELETE: api/radicado/{id}/otorgantes/{idRadicadoOtorgante}
        [HttpDelete("{id}/otorgantes/{idTercero}")]
        [RequirePermission(PermissionCodes.EditarRadicados)]
        public async Task<IActionResult> DeleteOtorgante(int id, int idTercero)
        {
            try
            {
                var entity = await _context.RadicadosOtorgantes
                    .FirstOrDefaultAsync(x => x.IdRadicado == id && x.IdTercero == idTercero);
                if (entity == null) return NotFound();

                _context.RadicadosOtorgantes.Remove(entity);
                await _context.SaveChangesAsync();
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
        [RequirePermission(PermissionCodes.VerRadicados)]
        public async Task<ActionResult<IEnumerable<InmuebleDto>>> GetInmuebles(int id)
        {
            try
            {
                var lista = await _context.ListaRadicadosInmuebles.Where(r => r.IdRadicado == id).ToListAsync();
                //.Radicados
                //.Include(r => r.RadicadosInmuebles)
                //.ThenInclude(i => i.IdInmuebleNavigation)
                //.ThenInclude(i => i.TipoInmueble)
                //.FirstOrDefaultAsync(r => r.IdRadicado == id);
                if (lista == null) return NotFound();

                //var lista = radicado.Where(i => i.ProyectoId == radicado.ProyectoId).ToList();
                return Ok(_mapper.Map<List<InmuebleDto>>(lista));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo inmuebles del radicado: {ex.Message}");
            }
        }

        // POST: api/radicado/{id}/inmuebles/{inmuebleId}
        [HttpPost("{id}/inmuebles/{inmuebleId}")]
        [RequirePermission(PermissionCodes.EditarRadicados)]
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
        [RequirePermission(PermissionCodes.EditarRadicados)]
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
        [RequirePermission(PermissionCodes.EditarRadicados)]
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
