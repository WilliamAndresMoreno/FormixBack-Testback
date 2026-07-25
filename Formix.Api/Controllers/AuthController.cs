using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Formix.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Formix.Api.Services;
using Formix.Domain.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Formix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IUserAuthorizationService _userAuth;

        public AuthController(IConfiguration configuration, AppDbContext context, IUserAuthorizationService userAuth)
        {
            _configuration = configuration;
            _context = context;
            _userAuth = userAuth;
        }

        public class LoginRequest
        {
            public int TenantId { get; set; }
            public string TenantName { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public LoginUserInfoDto? Usuario { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            //request.TenantId = 1;
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Usuario y contraseña son requeridos");

            var cleanUsername = request.Username.Trim().ToLower();
            var user = await _context.SysUsuarios.FirstOrDefaultAsync(u => u.NombreUsuario == cleanUsername && u.Activo == true && u.TenantId == request.TenantId);
            if (user == null)
                return Unauthorized("Usuario no encontrado o inactivo");

            // Verificar contraseña con PBKDF2 usando el salt almacenado
            if (user.PasswordSalt == null || user.PasswordHash == null)
                return Unauthorized("Credenciales inválidas");

            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                return Unauthorized("Credenciales inválidas");

            var jwtSection = _configuration.GetSection("Jwt");
            var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            var authInfo = await _userAuth.GetAuthorizationInfoAsync(user.UsuarioId, user.TenantId);
            var loginUser = await _userAuth.BuildLoginUserInfoAsync(user.UsuarioId, user.TenantId);

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var claims = JwtClaimsFactory.BuildClaims(
                user.UsuarioId, user.NombreUsuario, user.TenantId, authInfo);

            var expires = DateTime.UtcNow.AddHours(8);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            user.UltimoLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                Token = tokenString,
                ExpiresAt = expires,
                Usuario = loginUser
            });
        }

        /// <summary>Perfil y permisos efectivos del usuario autenticado.</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<LoginUserInfoDto>> Me()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var tenantClaim = User.FindFirstValue("tenant");

            if (!int.TryParse(sub, out var usuarioId) || !int.TryParse(tenantClaim, out var tenantId))
                return Unauthorized();

            var info = await _userAuth.BuildLoginUserInfoAsync(usuarioId, tenantId);
            if (info == null) return NotFound();

            return Ok(info);
        }

        private static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, 100_000, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(storedHash.Length);
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }
    }
}
