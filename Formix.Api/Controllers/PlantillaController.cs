using AutoMapper;
using Formix.API.Controllers;
using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Formix.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlantillaController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public PlantillaController(AppDbContext db, IMapper mapper, IWebHostEnvironment env)
        {
            _db = db;
            _mapper = mapper;
            _env = env;
        }

        // GET api/plantilla
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantillaDto>>> GetAll()
        {
            try
            {
                var items = await _db.Plantillas.AsNoTracking().ToListAsync();
                return Ok(_mapper.Map<IEnumerable<PlantillaDto>>(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo plantillas: {ex.Message}");
            }
        }

        // GET api/plantilla/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PlantillaDto>> GetById(int id)
        {
            try
            {
                var item = await _db.Plantillas.AsNoTracking().FirstOrDefaultAsync(p => p.IdPlantilla == id);
                if (item == null) return NotFound();
                return Ok(_mapper.Map<PlantillaDto>(item));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo la plantilla: {ex.Message}");
            }
        }

        // POST api/plantilla
        [HttpPost]
        public async Task<ActionResult<PlantillaDto>> Create([FromBody] PlantillaDto dto)
        {
            try
            {
                var entity = _mapper.Map<Plantilla>(dto);
                _db.Plantillas.Add(entity);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = entity.IdPlantilla }, _mapper.Map<PlantillaDto>(entity));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al crear la plantilla: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al crear la plantilla: {ex.Message}");
            }
        }

        // PUT api/plantilla/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<PlantillaDto>> Update(int id, [FromBody] PlantillaDto dto)
        {
            try
            {
                var entity = await _db.Plantillas.FirstOrDefaultAsync(p => p.IdPlantilla == id);
                if (entity == null) return NotFound();

                entity.Nombre = dto.Nombre ?? entity.Nombre;
                entity.Estado = dto.Estado;
                entity.Archivo = dto.Archivo ?? entity.Archivo;

                await _db.SaveChangesAsync();
                return Ok(_mapper.Map<PlantillaDto>(entity));
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Conflicto de concurrencia al actualizar la plantilla");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando la plantilla: {ex.Message}");
            }
        }

        // DELETE api/plantilla/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _db.Plantillas.FirstOrDefaultAsync(p => p.IdPlantilla == id);
                if (entity == null) return NotFound();

                // Attempt to delete physical file if present
                if (!string.IsNullOrWhiteSpace(entity.Archivo))
                {
                    var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var physicalPath = Path.Combine(root, entity.Archivo.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        try { System.IO.File.Delete(physicalPath); } catch { /* ignore */ }
                    }
                }

                _db.Plantillas.Remove(entity);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al eliminar la plantilla: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al eliminar la plantilla: {ex.Message}");
            }
        }

        // POST api/plantilla/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PlantillaDto>> Upload([FromForm] UploadPlantillaRequest request)
        {
            try
            {
                if (request.Archivo == null || request.Archivo.Length == 0)
                    return BadRequest("El archivo es obligatorio.");

                var extension = Path.GetExtension(request.Archivo.FileName).ToLowerInvariant();
                if (extension != ".doc" && extension != ".docx")
                    return BadRequest("El archivo debe ser un documento de Word (.doc o .docx).");

                var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(root, "plantillas");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var filename = $"{Guid.NewGuid()}" + extension;
                var physicalPath = Path.Combine(folder, filename);
                using (var fs = new FileStream(physicalPath, FileMode.Create))
                {
                    await request.Archivo.CopyToAsync(fs);
                }

                var relativePath = Path.Combine("plantillas", filename).Replace('\\', '/');

                var entity = new Plantilla
                {
                    Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? Path.GetFileNameWithoutExtension(request.Archivo.FileName) : request.Nombre!,
                    Estado = request.Estado,
                    Archivo = relativePath
                };

                _db.Plantillas.Add(entity);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = entity.IdPlantilla }, _mapper.Map<PlantillaDto>(entity));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al registrar la plantilla: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado en carga de plantilla: {ex.Message}");
            }
        }

        [HttpGet("{idPlantilla}/archivo/plantilla")]
        public async Task<IActionResult> DescargarArchivoPorIdPlantilla(int idPlantilla)
        {
            try
            {
                var plantilla = await _db.Plantillas.FindAsync(idPlantilla);
                if (plantilla == null || string.IsNullOrEmpty(plantilla.Archivo))
                    return NotFound();

                var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var physicalPath = Path.Combine(root, plantilla.Archivo.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!System.IO.File.Exists(physicalPath))
                    return NotFound("El archivo no existe en el servidor");

                var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return PhysicalFile(physicalPath, contentType, Path.GetFileName(physicalPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error descargando archivo de plantilla: {ex.Message}");
            }
        }

        [HttpGet("{idRadicado}/archivo/idradicado")]
        public async Task<IActionResult> DescargarArchivoPorIdRadicado(int idRadicado)
        {
            try
            {
                var radicado = await _db.Radicados.FindAsync(idRadicado);

                if (radicado == null)
                    return NotFound("El radicado no existe.");

                if (radicado.PlantillaId == null || radicado.PlantillaId <= 0)
                    return NotFound("No tiene plantilla relacionada.");

                var plantilla = await _db.Plantillas.FindAsync(radicado.PlantillaId);
                if (plantilla == null || string.IsNullOrEmpty(plantilla.Archivo))
                    return NotFound();

                var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var physicalPath = Path.Combine(root, plantilla.Archivo.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!System.IO.File.Exists(physicalPath))
                    return NotFound("El archivo no existe en el servidor");

                var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return PhysicalFile(physicalPath, contentType, Path.GetFileName(physicalPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error descargando archivo por radicado: {ex.Message}");
            }
        }

        [HttpGet("{numeroRadicado}/archivo/numeroradicado")]
        public async Task<IActionResult> DescargarArchivoPorNumeroRadicado(string numeroRadicado)
        {
            try
            {
                var radicado = await _db.Radicados.Where(r => r.Consecutivo == numeroRadicado).FirstOrDefaultAsync();

                if (radicado == null)
                    return NotFound("El radicado no existe.");

                if (radicado.PlantillaId == null || radicado.PlantillaId <= 0)
                    return NotFound("No tiene plantilla relacionada.");

                var plantilla = await _db.Plantillas.FindAsync(radicado.PlantillaId);
                if (plantilla == null || string.IsNullOrEmpty(plantilla.Archivo))
                    return NotFound();

                var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var physicalPath = Path.Combine(root, plantilla.Archivo.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!System.IO.File.Exists(physicalPath))
                    return NotFound("El archivo no existe en el servidor");

                var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return PhysicalFile(physicalPath, contentType, Path.GetFileName(physicalPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error descargando archivo por número de radicado: {ex.Message}");
            }
        }


        // PUT api/plantilla/{id}/upload
        [HttpPut("{id:int}/upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PlantillaDto>> ReplaceFile(int id, ReplacePlantillaFileRequest request)
        {
            try
            {
                var entity = await _db.Plantillas.FirstOrDefaultAsync(p => p.IdPlantilla == id);
                if (entity == null) return NotFound();
                if (request.Archivo == null || request.Archivo.Length == 0) return BadRequest("El archivo es obligatorio.");

                var extension = Path.GetExtension(request.Archivo.FileName).ToLowerInvariant();
                if (extension != ".doc" && extension != ".docx")
                    return BadRequest("El archivo debe ser un documento de Word (.doc o .docx).");

                var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(root, "plantillas");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                // Delete previous file if exists
                if (!string.IsNullOrWhiteSpace(entity.Archivo))
                {
                    var prevPath = Path.Combine(root, entity.Archivo.Replace('/', Path.DirectorySeparatorChar));
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

                var relativePath = Path.Combine("plantillas", filename).Replace('\\', '/');
                entity.Archivo = relativePath;
                await _db.SaveChangesAsync();

                return Ok(_mapper.Map<PlantillaDto>(entity));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al reemplazar archivo de plantilla: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado al reemplazar archivo: {ex.Message}");
            }
        }
    }
}
