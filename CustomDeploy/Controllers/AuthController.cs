using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomDeploy.Models;
using CustomDeploy.Services.Business;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUsuarioBusinessService _usuarioService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IOptions<JwtSettings> jwtSettings,
            IUsuarioBusinessService usuarioService,
            ILogger<AuthController> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Email e senha são obrigatórios" });
                }

                // Autenticar usuário
                var usuario = await _usuarioService.AutenticarAsync(request.Username, request.Password);
                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de login inválida para: {Email}", request.Username);
                    return Unauthorized(new { message = "Credenciais inválidas" });
                }

                // Gerar token
                var token = GenerateToken(usuario);
                var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

                _logger.LogInformation("Login realizado com sucesso para: {Email}", request.Username);

                return Ok(new LoginResponse
                {
                    Token = token,
                    Expiration = expiration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o login para: {Email}", request.Username);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("validate-token")]
        [Authorize]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                // Se chegou até aqui, o token é válido (middleware de Authorization já verificou)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Token inválido", isValid = false });
                }

                // Verificar se o usuário ainda existe e está ativo
                var usuario = await _usuarioService.ObterUsuarioPorIdAsync(userId);
                if (usuario == null || !usuario.Ativo)
                {
                    return Unauthorized(new { message = "Usuário não encontrado ou inativo", isValid = false });
                }
                
                return Ok(new { 
                    message = "Token is valid", 
                    email = email,
                    userId = userId,
                    nome = usuario.Nome,
                    acessoNivel = usuario.UsuarioAcesso.AcessoNivel.Nome,
                    isValid = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na validação do token");
                return Unauthorized(new { 
                    message = "Token is invalid", 
                    isValid = false,
                    error = ex.Message 
                });
            }
        }

        private string GenerateToken(CustomDeploy.Models.Entities.Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.UsuarioAcesso.AcessoNivel.Nome),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
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
