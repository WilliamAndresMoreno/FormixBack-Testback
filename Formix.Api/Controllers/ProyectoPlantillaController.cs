using AutoMapper;
using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Formix.Api.Authorization;
using iText.Layout.Borders;

namespace Formix.API.Controllers
{
    public class ProyectoPlantillaAssignRequest
    {
        public int ProyectoId { get; set; }
        public int PlantillaId { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProyectoPlantillaController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public ProyectoPlantillaController(AppDbContext db, IMapper mapper, IWebHostEnvironment env, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _env = env;
            _configuration = configuration;
        }

        // GET api/proyectoplantilla/proyecto/{proyectoId}
        [HttpGet("proyecto/{proyectoId:int}")]
        [RequirePermission(PermissionCodes.VerPlantillas)]
        public async Task<ActionResult<IEnumerable<ProyectoPlantillaDto>>> GetByProyecto(int proyectoId)
        {
            try
            {
                var items = await _db.ProyectoPlantillas
                    .AsNoTracking()
                    .Where(pp => pp.ProyectoId == proyectoId)
                    .ToListAsync();

                if (items == null || items.Count == 0) return Ok(Enumerable.Empty<ProyectoPlantillaDto>());

                var dtos = _mapper.Map<IEnumerable<ProyectoPlantillaDto>>(items);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo plantillas del proyecto: {ex.Message}");
            }
        }

        // POST api/proyectoplantilla
        [HttpPost]
        [RequirePermission(PermissionCodes.GestionarPlantillas)]
        public async Task<IActionResult> Asignar([FromBody] ProyectoPlantillaAssignRequest request)
        {
            try
            {
                // Verificar proyecto existente
                var proyectoExiste = await _db.Proyectos.AnyAsync(p => p.ProyectoId == request.ProyectoId);
                if (!proyectoExiste) return NotFound($"Proyecto {request.ProyectoId} no encontrado");

                // Si ya existe la asociación, devolver OK idempotente
                var yaExiste = await _db.ProyectoPlantillas.AnyAsync(pl => pl.ProyectoId == request.ProyectoId && pl.PlantillaId == request.PlantillaId);
                if (yaExiste) return Ok();

                // Crear la plantilla del proyecto con valores por defecto si no existe
                var nuevo = new ProyectoPlantilla
                {
                    ProyectoId = request.ProyectoId,
                    PlantillaId = request.PlantillaId,
                    Nombre = $"Plantilla {request.PlantillaId}",
                    Archivo = string.Empty,
                    Estado = 1
                };

                _db.ProyectoPlantillas.Add(nuevo);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByProyecto), new { proyectoId = request.ProyectoId }, _mapper.Map<ProyectoPlantillaDto>(nuevo));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al asignar plantilla al proyecto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al asignar plantilla: {ex.Message}");
            }
        }

        // DELETE api/proyectoplantilla/{proyectoId}/{plantillaId}
        [HttpDelete("{proyectoId:int}/{plantillaId:int}")]
        [RequirePermission(PermissionCodes.GestionarPlantillas)]
        public async Task<IActionResult> Quitar(int proyectoId, int plantillaId)
        {
            try
            {
                var plantilla = await _db.ProyectoPlantillas.FirstOrDefaultAsync(pl => pl.ProyectoId == proyectoId && pl.PlantillaId == plantillaId);
                if (plantilla == null) return NotFound($"Plantilla {plantillaId} no asignada al proyecto {proyectoId}");

                _db.ProyectoPlantillas.Remove(plantilla);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al quitar plantilla del proyecto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al quitar plantilla: {ex.Message}");
            }
        }

        // GET api/proyectoplantilla/{proyectoId}/{plantillaId}
        [HttpGet("{proyectoId:int}/{plantillaId:int}")]
        [RequirePermission(PermissionCodes.VerPlantillas)]
        public async Task<ActionResult<ProyectoPlantillaDto>> Get(int proyectoId, int plantillaId)
        {
            try
            {
                var item = await _db.ProyectoPlantillas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(pl => pl.ProyectoId == proyectoId && pl.PlantillaId == plantillaId);
                if (item == null) return NotFound();
                return Ok(_mapper.Map<ProyectoPlantillaDto>(item));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo la plantilla del proyecto: {ex.Message}");
            }
        }

        // POST api/proyectoplantilla/crear
        [HttpPost("crear")]
        [RequirePermission(PermissionCodes.GestionarPlantillas)]
        public async Task<ActionResult<ProyectoPlantillaDto>> Crear([FromBody] ProyectoPlantillaDto dto)
        {
            try
            {
                var proyectoExiste = await _db.Proyectos.AnyAsync(p => p.ProyectoId == dto.ProyectoId);
                if (!proyectoExiste) return NotFound($"Proyecto {dto.ProyectoId} no encontrado");

                var yaExiste = await _db.ProyectoPlantillas.AnyAsync(pl => pl.ProyectoId == dto.ProyectoId && pl.PlantillaId == dto.PlantillaId);
                if (yaExiste) return Conflict($"Ya existe la plantilla {dto.PlantillaId} para el proyecto {dto.ProyectoId}");

                var entity = new ProyectoPlantilla
                {
                    ProyectoId = dto.ProyectoId,
                    PlantillaId = dto.PlantillaId,
                    Nombre = dto.Nombre ?? $"Plantilla {dto.PlantillaId}",
                    Archivo = dto.Archivo ?? string.Empty,
                    Estado = dto.Estado
                };
                _db.ProyectoPlantillas.Add(entity);
                await _db.SaveChangesAsync();
                var result = _mapper.Map<ProyectoPlantillaDto>(entity);
                return CreatedAtAction(nameof(Get), new { proyectoId = entity.ProyectoId, plantillaId = entity.PlantillaId }, result);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear plantilla del proyecto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear plantilla del proyecto: {ex.Message}");
            }
        }

        // PUT api/proyectoplantilla/{proyectoId}/{plantillaId}
        [HttpPut("{proyectoId:int}/{plantillaId:int}")]
        [RequirePermission(PermissionCodes.GestionarPlantillas)]
        public async Task<ActionResult<ProyectoPlantillaDto>> Update(int proyectoId, int plantillaId, [FromBody] ProyectoPlantillaDto dto)
        {
            try
            {
                var entity = await _db.ProyectoPlantillas.FirstOrDefaultAsync(pl => pl.ProyectoId == proyectoId && pl.PlantillaId == plantillaId);
                if (entity == null) return NotFound($"Plantilla {plantillaId} no asignada al proyecto {proyectoId}");

                entity.Nombre = dto.Nombre ?? entity.Nombre;
                entity.Archivo = dto.Archivo ?? entity.Archivo;
                entity.Estado = dto.Estado;

                await _db.SaveChangesAsync();
                return Ok(_mapper.Map<ProyectoPlantillaDto>(entity));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al actualizar plantilla del proyecto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al actualizar plantilla del proyecto: {ex.Message}");
            }
        }

        // POST api/proyectoplantilla/upload
        [HttpPost("upload")]
        [RequirePermission(PermissionCodes.GestionarPlantillas)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProyectoPlantillaDto>> Upload([FromForm] UploadProyectoPlantillaRequest request)
        {
            try
            {
                if (request.Archivo == null || request.Archivo.Length == 0)
                    return BadRequest("El archivo es obligatorio.");

                var extension = Path.GetExtension(request.Archivo.FileName).ToLowerInvariant();
                if (extension != ".doc" && extension != ".docx")
                    return BadRequest("El archivo debe ser un documento de Word (.doc o .docx).");

                //var root = _configuration.GetValue<string>("Storage:PlantillasPath");//_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                //var folder = Path.Combine(root, "plantillas");
                //if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                //var filename = $"{Guid.NewGuid()}" + extension;
                //var physicalPath = Path.Combine(folder, filename);
                //using (var fs = new FileStream(physicalPath, FileMode.Create))
                //{
                //    await request.Archivo.CopyToAsync(fs);
                //}

                var configuredPath = _configuration["Storage:BasePath"];

                string folder;

                if (Path.IsPathRooted(configuredPath))
                {
                    folder = configuredPath;
                }
                else
                {
                    folder = Path.Combine(_env.ContentRootPath, configuredPath);
                }

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var filename = $"{Guid.NewGuid()}{extension}";
                var physicalPath = Path.Combine(folder, filename);

                using (var fs = new FileStream(physicalPath, FileMode.Create))
                {
                    await request.Archivo.CopyToAsync(fs);
                }

                var relativePath =filename.Replace('\\', '/');

                var entity = await _db.ProyectoPlantillas.FirstOrDefaultAsync(pl => pl.ProyectoId == request.ProyectoId && pl.PlantillaId == request.PlantillaId);
                if (entity == null)
                {
                    // Crear nueva asociación si no existe
                    entity = new ProyectoPlantilla
                    {
                        ProyectoId = request.ProyectoId,
                        //PlantillaId = ()request.PlantillaId,
                        Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? Path.GetFileNameWithoutExtension(request.Archivo.FileName) : request.Nombre!,
                        Estado = request.Estado ?? 1,
                        Archivo = relativePath
                    };
                    _db.ProyectoPlantillas.Add(entity);
                }
                else
                {
                    entity.Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? entity.Nombre : request.Nombre!;
                    entity.Estado = request.Estado ?? entity.Estado;
                    // Eliminar archivo anterior si existe
                    if (!string.IsNullOrWhiteSpace(entity.Archivo))
                    {
                        var prevPath = Path.Combine(configuredPath, entity.Archivo.Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(prevPath))
                        {
                            try { System.IO.File.Delete(prevPath); } catch { /* ignore */ }
                        }
                    }
                    entity.Archivo = relativePath;
                }

                await _db.SaveChangesAsync();
                //return CreatedAtAction(nameof(Get), new { proyectoId = entity.ProyectoId, plantillaId = entity.PlantillaId }, _mapper.Map<ProyectoPlantillaDto>(entity));
                return Ok(new
                {
                    WebRoot = _env.WebRootPath,
                    CurrentDir = Directory.GetCurrentDirectory()
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al registrar la plantilla del proyecto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado en carga de plantilla del proyecto: {ex.Message}");
            }
        }

        // PUT api/proyectoplantilla/{proyectoId}/{plantillaId}/upload
        [HttpPut("{proyectoId:int}/{plantillaId:int}/upload")]
        [RequirePermission(PermissionCodes.GestionarPlantillas)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProyectoPlantillaDto>> ReplaceFile(int proyectoId, int plantillaId, [FromForm] ReplaceProyectoPlantillaFileRequest request)
        {
            try
            {
                var entity = await _db.ProyectoPlantillas.FirstOrDefaultAsync(pl => pl.ProyectoId == proyectoId && pl.PlantillaId == plantillaId);
                if (entity == null) return NotFound();
                if (request.Archivo == null || request.Archivo.Length == 0) return BadRequest("El archivo es obligatorio.");

                var extension = Path.GetExtension(request.Archivo.FileName).ToLowerInvariant();
                if (extension != ".doc" && extension != ".docx")
                    return BadRequest("El archivo debe ser un documento de Word (.doc o .docx).");

                var configuredPath = _configuration["Storage:BasePath"];

                string folder;

                if (Path.IsPathRooted(configuredPath))
                {
                    folder = configuredPath;
                }
                else
                {
                    folder = Path.Combine(_env.ContentRootPath, configuredPath);
                }

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Delete previous file if exists
                if (!string.IsNullOrWhiteSpace(entity.Archivo))
                {
                    var prevPath = Path.Combine(configuredPath, entity.Archivo.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(prevPath))
                    {
                        try { System.IO.File.Delete(prevPath); } catch { /* ignore */ }
                    }
                }

                var filename = $"{Guid.NewGuid()}" + extension;
                var physicalPath = Path.Combine(folder, filename);
                using (var fs = new FileStream(physicalPath, FileMode.Create))
                {
                    await request.Archivo.CopyToAsync(fs);
                }

                var relativePath = filename.Replace('\\', '/');
                entity.Archivo = relativePath;
                await _db.SaveChangesAsync();

                return Ok(_mapper.Map<ProyectoPlantillaDto>(entity));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al reemplazar archivo de plantilla del proyecto: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al reemplazar archivo: {ex.Message}");
            }
        }

        // GET api/proyectoplantilla/{proyectoId}/{plantillaId}/archivo
        [HttpGet("{proyectoId:int}/{plantillaId:int}/archivo")]
        [RequirePermission(PermissionCodes.VerPlantillas)]
        public async Task<IActionResult> DescargarArchivo(int proyectoId, int plantillaId)
        {
            try
            {
                var entity = await _db.ProyectoPlantillas.AsNoTracking().FirstOrDefaultAsync(pl => pl.ProyectoId == proyectoId && pl.PlantillaId == plantillaId);
                if (entity == null || string.IsNullOrEmpty(entity.Archivo))
                    return NotFound();

                //var root = _configuration.GetValue<string>("Storage:PlantillasPath");//_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                //var physicalPath = Path.Combine(root, entity.Archivo.Replace("/", Path.DirectorySeparatorChar.ToString()));

                var configuredPath = _configuration["Storage:BasePath"];

                string folder;

                if (Path.IsPathRooted(configuredPath))
                {
                    folder = configuredPath;
                }
                else
                {
                    folder = Path.Combine(_env.ContentRootPath, configuredPath);
                }

                var physicalPath = Path.Combine(folder, entity.Archivo);

                if (!System.IO.File.Exists(physicalPath))
                    return NotFound("El archivo no existe en el servidor");

                var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return PhysicalFile(physicalPath, contentType, Path.GetFileName(physicalPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error descargando archivo de plantilla del proyecto: {ex.Message}");
            }
        }

        // GET api/proyectoplantilla/radicado/{idRadicado}/archivo
        [HttpGet("radicado/{idRadicado:int}/archivo")]
        [RequirePermission(PermissionCodes.VerPlantillas)]
        public async Task<IActionResult> DescargarArchivoPorIdRadicado(int idRadicado)
        {
            try
            {
                var radicado = await _db.Radicados.FindAsync(idRadicado);
                if (radicado == null) return NotFound("El radicado no existe.");
                if (radicado.PlantillaId == null || radicado.PlantillaId <= 0) return NotFound("No tiene plantilla relacionada.");

                var entity = await _db.ProyectoPlantillas.AsNoTracking().FirstOrDefaultAsync(pl => pl.ProyectoId == radicado.ProyectoId && pl.PlantillaId == radicado.PlantillaId);
                if (entity == null || string.IsNullOrEmpty(entity.Archivo)) return NotFound();

                //var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                //var physicalPath = Path.Combine(root, entity.Archivo.Replace("/", Path.DirectorySeparatorChar.ToString()));

                var configuredPath = _configuration["Storage:BasePath"];

                string folder;

                if (Path.IsPathRooted(configuredPath))
                {
                    folder = configuredPath;
                }
                else
                {
                    folder = Path.Combine(_env.ContentRootPath, configuredPath);
                }

                var physicalPath = Path.Combine(folder, entity.Archivo);


                if (!System.IO.File.Exists(physicalPath)) return NotFound("El archivo no existe en el servidor");

                var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return PhysicalFile(physicalPath, contentType, Path.GetFileName(physicalPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error descargando archivo por radicado: {ex.Message}");
            }
        }

        // GET api/proyectoplantilla/radicado/numero/{numeroRadicado}/archivo
        [HttpGet("radicado/numero/{numeroRadicado}/archivo")]
        [RequirePermission(PermissionCodes.VerPlantillas)]
        public async Task<IActionResult> DescargarArchivoPorNumeroRadicado(string numeroRadicado)
        {
            try
            {
                var radicado = await _db.Radicados.FirstOrDefaultAsync(r => r.Consecutivo == numeroRadicado);
                if (radicado == null) return NotFound("El radicado no existe.");
                if (radicado.PlantillaId == null || radicado.PlantillaId <= 0) return NotFound("No tiene plantilla relacionada.");

                var entity = await _db.ProyectoPlantillas.AsNoTracking().FirstOrDefaultAsync(pl => pl.ProyectoId == radicado.ProyectoId && pl.PlantillaId == radicado.PlantillaId);
                if (entity == null || string.IsNullOrEmpty(entity.Archivo)) return NotFound();

                //var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                //var physicalPath = Path.Combine(root, entity.Archivo.Replace("/", Path.DirectorySeparatorChar.ToString()));

                var configuredPath = _configuration["Storage:BasePath"];

                string folder;

                if (Path.IsPathRooted(configuredPath))
                {
                    folder = configuredPath;
                }
                else
                {
                    folder = Path.Combine(_env.ContentRootPath, configuredPath);
                }

                var physicalPath = Path.Combine(folder, entity.Archivo);

                if (!System.IO.File.Exists(physicalPath)) return NotFound("El archivo no existe en el servidor");

                var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return PhysicalFile(physicalPath, contentType, Path.GetFileName(physicalPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error descargando archivo por número de radicado: {ex.Message}");
            }
        }

        // GET api/proyectoplantilla/proyecto/plantilla/{idProyecto}/{nombrePlantilla}/archivo
        [HttpGet("proyecto/plantilla/{idProyecto}/{nombrePlantilla}/archivo")]
        [RequirePermission(PermissionCodes.VerPlantillas)]
        public async Task<IActionResult> DescargarArchivoPorNombrePlantilla(int idProyecto, string nombrePlantilla)
        {
            try
            {
                var entity = await _db.ProyectoPlantillas.AsNoTracking().FirstOrDefaultAsync(pl => pl.ProyectoId == idProyecto && pl.Nombre.Trim().ToUpper() == nombrePlantilla.Trim().ToUpper());
                if (entity == null || string.IsNullOrEmpty(entity.Archivo))
                    return NotFound();

                //var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                //var physicalPath = Path.Combine(root, entity.Archivo.Replace("/", Path.DirectorySeparatorChar.ToString()));

                var configuredPath = _configuration["Storage:BasePath"];

                string folder;

                if (Path.IsPathRooted(configuredPath))
                {
                    folder = configuredPath;
                }
                else
                {
                    folder = Path.Combine(_env.ContentRootPath, configuredPath);
                }

                var physicalPath = Path.Combine(folder, entity.Archivo);

                if (!System.IO.File.Exists(physicalPath))
                    return NotFound("El archivo no existe en el servidor");

                var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return PhysicalFile(physicalPath, contentType, Path.GetFileName(physicalPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error descargando archivo de plantilla del proyecto: {ex.Message}");
            }
        }
    }
}
