using AutoMapper;
using Formix.Domain.Dtos;
using Formix.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formix.Api.Authorization;

namespace Formix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TiposEstadoCivilController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TiposEstadoCivilController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/tiposestadocivil
        [HttpGet]
        [RequirePermission(PermissionCodes.VerCatalogos)]
        public async Task<ActionResult<IEnumerable<TiposEstadoCivilDto>>> GetAll()
        {
            try
            {
                var lista = await _context.TiposEstadoCivils
                    .OrderBy(t => t.EstadoCivil)
                    .ToListAsync();

                return Ok(_mapper.Map<IEnumerable<TiposEstadoCivilDto>>(lista));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo tipos de estado civil: {ex.Message}");
            }
        }
    }
}
