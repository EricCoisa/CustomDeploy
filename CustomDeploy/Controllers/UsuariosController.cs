using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CustomDeploy.Services.Business;
using CustomDeploy.Models.DTOs;
using System.Security.Claims;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioBusinessService _usuarioService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IUsuarioBusinessService usuarioService, ILogger<UsuariosController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os usuários
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<UsuarioResponse>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _usuarioService.ObterTodosUsuariosAsync();
                var response = usuarios.Select(u => new UsuarioResponse
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Ativo = u.Ativo,
                    AcessoNivel = u.UsuarioAcesso.AcessoNivel.Nome,
                    CriadoEm = u.CriadoEm,
                    AtualizadoEm = u.AtualizadoEm
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuários");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obtém um usuário por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponse>> GetUsuario(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                // Usuários só podem ver seus próprios dados, exceto administradores
                if (!isAdmin && currentUserId != id)
                {
                    return Forbid("Você só pode visualizar seus próprios dados");
                }

                var usuario = await _usuarioService.ObterUsuarioPorIdAsync(id);
                if (usuario == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                var response = new UsuarioResponse
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Ativo = usuario.Ativo,
                    AcessoNivel = usuario.UsuarioAcesso.AcessoNivel.Nome,
                    CriadoEm = usuario.CriadoEm,
                    AtualizadoEm = usuario.AtualizadoEm
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UsuarioResponse>> CriarUsuario(CriarUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.CriarUsuarioAsync(
                    request.Nome,
                    request.Email,
                    request.Senha,
                    request.AcessoNivelId
                );

                var response = new UsuarioResponse
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Ativo = usuario.Ativo,
                    AcessoNivel = usuario.UsuarioAcesso.AcessoNivel.Nome,
                    CriadoEm = usuario.CriadoEm,
                    AtualizadoEm = usuario.AtualizadoEm
                };

                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Atualiza um usuário
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarUsuario(int id, AtualizarUsuarioRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                // Usuários só podem atualizar seus próprios dados, exceto administradores
                if (!isAdmin && currentUserId != id)
                {
                    return Forbid("Você só pode atualizar seus próprios dados");
                }

                // Apenas administradores podem alterar o status Ativo
                if (!isAdmin && request.Ativo.HasValue)
                {
                    return Forbid("Apenas administradores podem alterar o status de ativo");
                }

                var resultado = await _usuarioService.AtualizarUsuarioAsync(
                    id, 
                    request.Nome, 
                    request.Email, 
                    request.Senha, 
                    request.Ativo
                );

                if (!resultado)
                {
                    return NotFound("Usuário não encontrado");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Exclui um usuário
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ExcluirUsuario(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == id)
                {
                    return BadRequest("Você não pode excluir seu próprio usuário");
                }

                var resultado = await _usuarioService.ExcluirUsuarioAsync(id);
                if (!resultado)
                {
                    return NotFound("Usuário não encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Verifica se um email já está em uso
        /// </summary>
        [HttpGet("verificar-email/{email}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<bool>> VerificarEmail(string email)
        {
            try
            {
                var existe = await _usuarioService.EmailExisteAsync(email);
                return Ok(existe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar email {Email}", email);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
