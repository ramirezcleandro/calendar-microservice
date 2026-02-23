using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CalendarioEntregas.WebApi.Controllers
{
    /// <summary>
    /// Controlador de prueba para generar tokens JWT. Solo para demo del proyecto final.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TestAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Genera un token JWT de prueba para el demo.
        /// </summary>
        [HttpGet("token")]
        public IActionResult GenerarTokenPrueba()
        {

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "usuario-prueba-id"),
                new Claim(JwtRegisteredClaimNames.Email, "prueba@nurtricenter.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expira = DateTime.UtcNow.AddHours(8),
                uso = "Authorization: Bearer " + tokenString
            });
        }
    }
}
