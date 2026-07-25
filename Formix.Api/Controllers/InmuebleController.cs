using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure;
using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Entities;
using Azure.Core;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using Formix.Api.Authorization;

namespace FormixBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InmuebleController : ControllerBase
    {
        public class InmuebleImportForm
        {
            public IFormFile? File { get; set; }
            public int ProyectoId { get; set; }
            public string tenantId { get; set; }
            public string? Formato { get; set; }
        }

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public InmuebleController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/inmueble
        [HttpGet]
        [RequirePermission(PermissionCodes.VerInmuebles)]
        public async Task<ActionResult<IEnumerable<InmuebleDto>>> GetInmuebles()
        {
            try
            {
                var lista = await _context.Inmuebles.OrderBy(o => o.MatriculaInmobiliaria).ToListAsync();
                var listaDto = _mapper.Map<List<InmuebleDto>>(lista);
                return Ok(listaDto);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = "Error en SQL", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
            }
        }

        // GET: api/inmueble/5
        [HttpGet("byProyecto/{idProyecto}")]
        [RequirePermission(PermissionCodes.VerInmuebles)]
        public async Task<ActionResult<IEnumerable<InmuebleDto>>> GetInmuebleByProyecto(int idProyecto)
        {
            try
            {
                var lista = await _context.Inmuebles.Where(i => i.ProyectoId == idProyecto).ToListAsync();
                var listaDto = _mapper.Map<List<InmuebleDto>>(lista);
                return Ok(listaDto);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = "Error en SQL", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
            }
        }

        // GET: api/inmueble/byProyecto/paged/5?page=1&pageSize=50
        [HttpGet("byProyecto/paged/{idProyecto}")]
        [RequirePermission(PermissionCodes.VerInmuebles)]
        public async Task<IActionResult> GetInmuebleByProyectoPaged(int idProyecto, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, string filter = "")
        {
            try
            {
                var query = _context.Inmuebles.Where(i => i.ProyectoId == idProyecto);

                if (!string.IsNullOrEmpty(filter))
                {
                    query = query.Where(f => f.Nombre.ToUpper().Contains(filter.ToUpper())
                    || f.MatriculaInmobiliaria.ToUpper().Contains(filter.ToUpper())
                    || f.CedulaCatastral.ToUpper().Contains(filter.ToUpper()));
                }

                var total = await query.CountAsync();

                if (pageSize <= 0)
                {
                    var all = await query.OrderBy(i => i.InmuebleId).ToListAsync();
                    var allDto = _mapper.Map<List<InmuebleDto>>(all);
                    return Ok(new { items = allDto, total = total });
                }

                var items = await query.OrderBy(i => i.InmuebleId).Skip((Math.Max(1, page) - 1) * pageSize).Take(pageSize).ToListAsync();
                var listaDto = _mapper.Map<List<InmuebleDto>>(items);

                return Ok(new { items = listaDto, total = total });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = "Error en SQL", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
            }
        }

        // POST: api/inmueble/fromfile
        [HttpPost("fromfile")]
        [RequirePermission(PermissionCodes.ImportarInmuebles)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> FromFile([FromForm] InmuebleImportForm form)
        {
            try
            {
                if (form?.File == null || form.File.Length == 0)
                    return BadRequest("Archivo no proporcionado.");

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using var stream = form.File.OpenReadStream();
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                });

                if (dataSet.Tables.Count == 0)
                    return BadRequest("El archivo no contiene hojas.");

                var tableSrc = dataSet.Tables[0];
                var table = new DataTable();

                if (form.Formato == "Colsubsidio")
                {
                    table.Columns.Add("TenantId", typeof(int));
                    table.Columns.Add("ProyectoId", typeof(int));
                    table.Columns.Add("Nombre", typeof(string));
                    table.Columns.Add("NombreCtl", typeof(string));
                    table.Columns.Add("NombreRph", typeof(string));
                    table.Columns.Add("TipoInmuebleId", typeof(int));
                    table.Columns.Add("Unidad", typeof(string));
                    table.Columns.Add("Numero", typeof(string));
                    table.Columns.Add("MatriculaInmobiliaria", typeof(string));
                    table.Columns.Add("CedulaCatastral", typeof(string));
                    table.Columns.Add("ChipCatastral", typeof(string));
                    table.Columns.Add("LinderoEspecial", typeof(string));
                    table.Columns.Add("Coeficiente", typeof(decimal));
                    table.Columns.Add("Direccion", typeof(string));
                    table.Columns.Add("ValorInmueble", typeof(decimal));
                    table.Columns.Add("SubsidioEntidad", typeof(string));
                    table.Columns.Add("SubsidioValor", typeof(decimal));
                    table.Columns.Add("SubsidioFchAsigna", typeof(DateTime));
                    table.Columns.Add("SubsidioFchAjusteAsigna", typeof(DateTime));
                    table.Columns.Add("SubsidioFchCartaProrroga", typeof(DateTime));
                    table.Columns.Add("CreditoEntidad", typeof(string));
                    table.Columns.Add("CreditoValor", typeof(decimal));
                    table.Columns.Add("CesantiasEntidad", typeof(string));
                    table.Columns.Add("CesantiasValor", typeof(decimal));
                    table.Columns.Add("AhorroEntidad", typeof(string));
                    table.Columns.Add("AhorroValor", typeof(decimal));
                    table.Columns.Add("FechaCreacion", typeof(DateTime));
                    table.Columns.Add("FechaActualizacion", typeof(DateTime));
                    table.Columns.Add("CompradorPrincipalNombre", typeof(string));
                    table.Columns.Add("CompradorPrincipalCedula", typeof(string));
                    table.Columns.Add("CompradorPrincipalCorreo", typeof(string));
                    table.Columns.Add("CompradorPrincipalCelular", typeof(string));
                    table.Columns.Add("Vendedor", typeof(string));
                    table.Columns.Add("CompradorAlternoNombre", typeof(string));
                    table.Columns.Add("CompradorAlternoCedula", typeof(string));
                    table.Columns.Add("CompradorAlternoCorreo", typeof(string));
                    table.Columns.Add("CompradorAlternoCelular", typeof(string));
                }
                else if (form.Formato == "Formix")
                {
                    table.Columns.Add("TenantId", typeof(int));
                    table.Columns.Add("ProyectoId", typeof(int));
                    table.Columns.Add("MatriculaInmobiliaria", typeof(string));
                    table.Columns.Add("CedulaCatastral", typeof(string));
                    table.Columns.Add("Nombre", typeof(string));
                    table.Columns.Add("Coeficiente", typeof(decimal));
                    table.Columns.Add("LinderoEspecial", typeof(string));
                    table.Columns.Add("TipoInmueble", typeof(string));
                    table.Columns.Add("ChipCatastral", typeof(string));
                    table.Columns.Add("Direccion", typeof(string));
                    table.Columns.Add("ValorInmueble", typeof(string));
                    table.Columns.Add("Unidad", typeof(string));
                    table.Columns.Add("Numero", typeof(string));
                }

                foreach (DataRow row in tableSrc.Rows)
                {
                    object GetObj(string col)
                    {
                        var column = tableSrc.Columns
                            .Cast<DataColumn>()
                            .FirstOrDefault(c =>
                                c.ColumnName.Trim()
                                    .Equals(col.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (column == null)
                            return DBNull.Value;

                        var v = row[column];
                        if (v == null || v == DBNull.Value)
                        {
                            //if (column.ToString() == "ValorInmueble")
                            //{
                            //    return 0.00;
                            //}
                            return DBNull.Value;
                        }


                        return v;
                    }

                    string GetStr(string col)
                    {
                        var o = GetObj(col);
                        return o == DBNull.Value ? string.Empty : o.ToString() ?? string.Empty;
                    }

                    //decimal GetDec(string col)
                    //{
                    //    var o = GetObj(col);//.ToString().Replace(",", "").Trim();
                    //    if (o == DBNull.Value) { return (decimal)0.00; }
                    //    var s = o.ToString().Trim();
                    //    //return decimal.TryParse(s, out var d) ? d : 0m;
                    //    //return decimal.Parse(s);
                    //    return decimal.Parse(s, CultureInfo.InvariantCulture);
                    //}

                    decimal GetDec(string col)
                    {
                        var o = GetObj(col);
                        if (o == DBNull.Value || o == "0") return 0m;

                        var s = o.ToString().Trim();

                        s = s.Replace(",", ".");

                        if (!decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        {
                            throw new Exception($"Error convirtiendo columna {col} con valor '{s}'");
                        }

                        return d;
                    }

                    object GetDateObj(string col)
                    {
                        var o = GetObj(col);
                        if (o == DBNull.Value) return DBNull.Value;
                        if (o is DateTime dt) return dt.Date;
                        var s = o.ToString();
                        if (DateTime.TryParse(s, out var dt2)) return dt2.Date;
                        return DBNull.Value;
                    }

                    var now = DateTime.UtcNow;

                    if (!string.IsNullOrWhiteSpace(form.Formato) && form.Formato.Equals("Formix", StringComparison.OrdinalIgnoreCase))
                    {
                        table.Rows.Add(
                            form.tenantId,
                            form.ProyectoId,
                            GetStr("Matricula inmobiliaria"),
                            GetStr("Cedula Catastral"),
                            GetStr("Nombre del Inmueble"),
                            GetDec("Coeficiente"),
                            GetStr("Lindero Especial"),
                            GetStr("TipoInmueble"),
                            GetStr("ChipCatastral"),
                            GetStr("Direccion"),
                            GetDec("ValorInmueble"),
                            GetStr("Unidad"),
                            GetStr("Numero")
                        );
                    }
                    else if (form.Formato == "Colsubsidio")
                    {
                        table.Rows.Add(
                            form.tenantId,
                            form.ProyectoId,
                            GetStr("Agrupación"),
                            GetStr("NombreCTL"),
                            GetStr("NombreRPH"),
                            1,
                            string.Empty,
                            string.Empty,
                            GetStr("Matricula Inmobiliaria"),
                            GetStr("Cedula Catastral"),
                            GetStr("Chip Catastral"),
                            GetStr("Lindero Especial"),
                            GetDec("Coeficiente"),
                            GetStr("Dirección"),
                            GetDec("Valor Inmueble"),
                            GetStr("Ent Subsidio"),
                            GetDec("Vr Subsidio"),
                            GetDateObj("Fecha Subsidio"),
                            DBNull.Value,
                            DBNull.Value,
                            GetStr("Ent Crédito"),
                            GetDec("Vr Credito"),
                            GetStr("Ent Cesantías"),
                            GetDec("Vr Cesantias"),
                            GetStr("Ent Ahorro"),
                            GetDec("Vr Ahorro"),
                            now,
                            now,
                            GetStr("Comprador Principal Nombre"),
                            GetStr("Comprador Principal Cédula"),
                            GetStr("Comprador Principal Correo"),
                            GetStr("Comprador Principal Celular"),
                            GetStr("Vendedor"),
                            GetStr("Comprador Alterno Nombre"),
                            GetStr("Comprador Alterno Cédula"),
                            GetStr("Comprador Alterno Correo"),
                            GetStr("Comprador Alterno Celular")
                        );
                    }
                }

                if (!string.IsNullOrWhiteSpace(form.Formato) && form.Formato.Equals("Formix", StringComparison.OrdinalIgnoreCase))
                {
                    var param = new SqlParameter("@Inmuebles", SqlDbType.Structured)
                    {
                        TypeName = "dbo.InmuebleTableType_Formix",
                        Value = table
                    };

                    await _context.Database.ExecuteSqlRawAsync("EXEC BulkInsertInmuebles_Formix @Inmuebles", param);
                }
                else if (form.Formato == "Colsubsidio")
                {
                    var param = new SqlParameter("@Inmuebles", SqlDbType.Structured)
                    {
                        TypeName = "dbo.InmuebleTableType",
                        Value = table
                    };

                    await _context.Database.ExecuteSqlRawAsync("EXEC BulkInsertInmuebles_Colsubsidio @Inmuebles", param);
                }

                return Ok(new { message = $"{table.Rows.Count} inmuebles importados correctamente desde archivo." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = "Error en SQL", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
            }
        }

        // GET: api/inmueble/5
        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.VerInmuebles)]
        public async Task<ActionResult<InmuebleDto>> GetInmueble(int id)
        {
            try
            {
                var inmueble = await _context.Inmuebles.FindAsync(id);

                if (inmueble == null)
                {
                    return NotFound();
                }

                var inmuebleDto = _mapper.Map<InmuebleDto>(inmueble);
                return Ok(inmuebleDto);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = "Error en SQL", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
            }
        }

        // POST: api/inmueble
        [HttpPost]
        [RequirePermission(PermissionCodes.CrearInmuebles)]
        public async Task<ActionResult<InmuebleDto>> PostInmueble(InmuebleDto inmuebleDto)
        {
            try
            {
                var entity = _mapper.Map<Inmueble>(inmuebleDto);

                _context.Inmuebles.Add(entity);
                await _context.SaveChangesAsync();

                var dto = _mapper.Map<InmuebleDto>(entity);

                return CreatedAtAction(nameof(GetInmueble), new { id = dto.InmuebleId }, dto);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Error al crear el inmueble", detalle = ex.Message });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = "Error en SQL", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
            }
        }

        // PUT: api/inmueble/5
        [HttpPut("{id}")]
        [RequirePermission(PermissionCodes.EditarInmuebles)]
        public async Task<IActionResult> PutInmueble(int id, InmuebleDto inmuebleDto)
        {
            if (id != inmuebleDto.InmuebleId)
            {
                return BadRequest();
            }
            try
            {
                var entity = _mapper.Map<Inmueble>(inmuebleDto);
                _context.Entry(entity).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InmuebleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(409, new { message = "Conflicto de concurrencia al actualizar el inmueble" });
                }
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Error al actualizar el inmueble", detalle = ex.Message });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = "Error en SQL", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
            }
        }

        // DELETE: api/inmueble/5
        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.EliminarInmuebles)]
        public async Task<IActionResult> DeleteInmueble(int id)
        {
            try
            {
                var inmueble = await _context.Inmuebles.FindAsync(id);
                if (inmueble == null)
                {
                    return NotFound();
                }

                _context.Inmuebles.Remove(inmueble);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Error al eliminar el inmueble", detalle = ex.Message });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = "Error en SQL", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno", detalle = ex.Message });
            }
        }

        private bool InmuebleExists(int id)
        {
            return _context.Inmuebles.Any(e => e.InmuebleId == id);
        }
    }
}
