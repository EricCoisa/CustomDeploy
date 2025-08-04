using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomDeploy.Models;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;

        public AuthController(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Autenticação fixa para desenvolvimento
            if (request.Username != "admin" || request.Password != "password")
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = GenerateToken(request.Username);
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            return Ok(new LoginResponse
            {
                Token = token,
                Expiration = expiration
            });
        }

        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            try
            {
                // Se chegou até aqui, o token é válido (middleware de Authorization já verificou)
                var username = User.Identity?.Name ?? "Unknown";
                
                return Ok(new { 
                    message = "Token is valid", 
                    username = username,
                    isValid = true 
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { 
                    message = "Token is invalid", 
                    isValid = false,
                    error = ex.Message 
                });
            }
        }

        private string GenerateToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
