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
    public class DeploysController : ControllerBase
    {
        private readonly IDeployBusinessService _deployService;
        private readonly IUsuarioBusinessService _usuarioService;
        private readonly ILogger<DeploysController> _logger;

        public DeploysController(
            IDeployBusinessService deployService,
            IUsuarioBusinessService usuarioService,
            ILogger<DeploysController> logger)
        {
            _deployService = deployService;
            _usuarioService = usuarioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os deploys
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeployResumoResponse>>> GetDeploys()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                IEnumerable<CustomDeploy.Models.Entities.Deploy> deploys;

                if (isAdmin)
                {
                    deploys = await _deployService.ObterTodosDeploysAsync();
                }
                else
                {
                    deploys = await _deployService.ObterDeploysPorUsuarioAsync(currentUserId);
                }

                var response = deploys.Select(d => new DeployResumoResponse
                {
                    Id = d.Id,
                    SiteName = d.SiteName,
                    ApplicationName = d.ApplicationName,
                    Data = d.Data,
                    Status = d.Status,
                    UsuarioNome = d.Usuario.Nome,
                    Plataforma = d.Plataforma
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter deploys");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obtém um deploy específico com detalhes completos
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DeployResponse>> GetDeploy(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                var deploy = await _deployService.ObterDeployCompletoAsync(id);
                if (deploy == null)
                {
                    return NotFound("Deploy não encontrado");
                }

                // Verificar permissão
                if (!isAdmin && deploy.UsuarioId != currentUserId)
                {
                    return Forbid("Você só pode visualizar seus próprios deploys");
                }

                var response = new DeployResponse
                {
                    Id = deploy.Id,
                    SiteName = deploy.SiteName,
                    ApplicationName = deploy.ApplicationName,
                    Data = deploy.Data,
                    Status = deploy.Status,
                    Mensagem = deploy.Mensagem,
                    Plataforma = deploy.Plataforma,
                    Usuario = new UsuarioResponse
                    {
                        Id = deploy.Usuario.Id,
                        Nome = deploy.Usuario.Nome,
                        Email = deploy.Usuario.Email,
                        Ativo = deploy.Usuario.Ativo,
                        AcessoNivel = deploy.Usuario.UsuarioAcesso.AcessoNivel.Nome,
                        CriadoEm = deploy.Usuario.CriadoEm,
                        AtualizadoEm = deploy.Usuario.AtualizadoEm
                    },
                    Comandos = deploy.DeployComandos.Select(dc => new DeployComandoResponse
                    {
                        Id = dc.Id,
                        Comando = dc.Comando,
                        Ordem = dc.Ordem,
                        CriadoEm = dc.CriadoEm,
                        Status = dc.Status,
                        Mensagem = dc.Mensagem,
                        ExecutadoEm = dc.ExecutadoEm,
                        TerminalId = dc.TerminalId // Adicionado o campo TerminalId
                    }).ToList(),
                    Historico = deploy.DeployHistoricos.Select(dh => new DeployHistoricoResponse
                    {
                        Id = dh.Id,
                        Data = dh.Data,
                        Status = dh.Status,
                        Mensagem = dh.Mensagem
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter deploy {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Cria um novo deploy
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DeployResponse>> CriarDeploy(CriarDeployRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();

                var deploy = await _deployService.CriarDeployAsync(
                    request.SiteName,
                    request.ApplicationName,
                    currentUserId,
                    request.BuildCommand,
                    request.RepoUrl,
                    request.Branch,
                    request.BuildOutput,
                    request.Plataforma
                );

                // Buscar o deploy completo para retornar
                var deployCompleto = await _deployService.ObterDeployCompletoAsync(deploy.Id);

                var response = new DeployResponse
                {
                    Id = deployCompleto!.Id,
                    SiteName = deployCompleto.SiteName,
                    ApplicationName = deployCompleto.ApplicationName,
                    Data = deployCompleto.Data,
                    Status = deployCompleto.Status,
                    Mensagem = deployCompleto.Mensagem,
                    Plataforma = deployCompleto.Plataforma,
                    Usuario = new UsuarioResponse
                    {
                        Id = deployCompleto.Usuario.Id,
                        Nome = deployCompleto.Usuario.Nome,
                        Email = deployCompleto.Usuario.Email,
                        Ativo = deployCompleto.Usuario.Ativo,
                        AcessoNivel = deployCompleto.Usuario.UsuarioAcesso.AcessoNivel.Nome,
                        CriadoEm = deployCompleto.Usuario.CriadoEm,
                        AtualizadoEm = deployCompleto.Usuario.AtualizadoEm
                    },
                    Comandos = deployCompleto.DeployComandos.Select(dc => new DeployComandoResponse
                    {
                        Id = dc.Id,
                        Comando = dc.Comando,
                        Ordem = dc.Ordem,
                        CriadoEm = dc.CriadoEm,
                        Status = dc.Status,
                        Mensagem = dc.Mensagem,
                        ExecutadoEm = dc.ExecutadoEm,
                        TerminalId = dc.TerminalId // Adicionado o campo TerminalId
                    }).ToList(),
                    Historico = deployCompleto.DeployHistoricos.Select(dh => new DeployHistoricoResponse
                    {
                        Id = dh.Id,
                        Data = dh.Data,
                        Status = dh.Status,
                        Mensagem = dh.Mensagem
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetDeploy), new { id = deploy.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar deploy");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Cria e executa um deploy completo (IIS + banco)
        /// </summary>
        [HttpPost("executar")]
        public async Task<ActionResult<DeployResponse>> ExecutarDeploy(CriarDeployCompletoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();

                // Converter para DeployRequest (modelo antigo)
                var deployRequest = new CustomDeploy.Models.DeployRequest
                {
                    RepoUrl = request.RepoUrl,
                    Branch = request.Branch,
                    BuildCommand = request.BuildCommand,
                    BuildOutput = request.BuildOutput,
                    IisSiteName = request.IisSiteName,
                    TargetPath = request.TargetPath,
                    ApplicationPath = request.ApplicationPath
                };

                // Executar deploy completo (IIS + banco)
                var deploy = await _deployService.ExecuteDeployCompletoAsync(deployRequest, currentUserId);

                // Buscar o deploy completo para retornar
                var deployCompleto = await _deployService.ObterDeployCompletoAsync(deploy.Id);

                var response = new DeployResponse
                {
                    Id = deployCompleto!.Id,
                    SiteName = deployCompleto.SiteName,
                    ApplicationName = deployCompleto.ApplicationName,
                    Data = deployCompleto.Data,
                    Status = deployCompleto.Status,
                    Mensagem = deployCompleto.Mensagem,
                    Plataforma = deployCompleto.Plataforma,
                    RepoUrl = request.RepoUrl,
                    Branch = request.Branch,
                    BuildCommands = request.BuildCommand,
                    BuildOutput = request.BuildOutput,
                    TargetPath = request.TargetPath,
                    ApplicationPath = request.ApplicationPath,
                    Usuario = new UsuarioResponse
                    {
                        Id = deployCompleto.Usuario.Id,
                        Nome = deployCompleto.Usuario.Nome,
                        Email = deployCompleto.Usuario.Email,
                        Ativo = deployCompleto.Usuario.Ativo,
                        AcessoNivel = deployCompleto.Usuario.UsuarioAcesso.AcessoNivel.Nome,
                        CriadoEm = deployCompleto.Usuario.CriadoEm,
                        AtualizadoEm = deployCompleto.Usuario.AtualizadoEm
                    },
                    Comandos = deployCompleto.DeployComandos.Select(dc => new DeployComandoResponse
                    {
                        Id = dc.Id,
                        Comando = dc.Comando,
                        Ordem = dc.Ordem,
                        CriadoEm = dc.CriadoEm,
                        Status = dc.Status,
                        Mensagem = dc.Mensagem,
                        ExecutadoEm = dc.ExecutadoEm,
                        TerminalId = dc.TerminalId // Adicionado o campo TerminalId
                    }).ToList(),
                    Historico = deployCompleto.DeployHistoricos.Select(dh => new DeployHistoricoResponse
                    {
                        Id = dh.Id,
                        Data = dh.Data,
                        Status = dh.Status,
                        Mensagem = dh.Mensagem
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetDeploy), new { id = deploy.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar deploy");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Atualiza o status de um deploy
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> AtualizarStatusDeploy(int id, AtualizarStatusDeployRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                var deploy = await _deployService.ObterDeployPorIdAsync(id);
                if (deploy == null)
                {
                    return NotFound("Deploy não encontrado");
                }

                // Verificar permissão
                if (!isAdmin && deploy.UsuarioId != currentUserId)
                {
                    return Forbid("Você só pode atualizar seus próprios deploys");
                }

                var resultado = await _deployService.AtualizarStatusDeployAsync(
                    id,
                    request.Status,
                    request.Mensagem
                );

                if (!resultado)
                {
                    return NotFound("Deploy não encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do deploy {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obtém deploys por site
        /// </summary>
        [HttpGet("site/{siteName}")]
        public async Task<ActionResult<IEnumerable<DeployResumoResponse>>> GetDeploysPorSite(string siteName)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                var deploys = await _deployService.ObterDeploysPorSiteAsync(siteName);

                // Filtrar por usuário se não for admin
                if (!isAdmin)
                {
                    deploys = deploys.Where(d => d.UsuarioId == currentUserId);
                }

                var response = deploys.Select(d => new DeployResumoResponse
                {
                    Id = d.Id,
                    SiteName = d.SiteName,
                    ApplicationName = d.ApplicationName,
                    Data = d.Data,
                    Status = d.Status,
                    UsuarioNome = d.Usuario.Nome,
                    Plataforma = d.Plataforma
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter deploys por site {SiteName}", siteName);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obtém deploys recentes
        /// </summary>
        [HttpGet("recentes")]
        public async Task<ActionResult<IEnumerable<DeployResumoResponse>>> GetDeploysRecentes([FromQuery] int quantidade = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                var deploys = await _deployService.ObterDeploysRecentesAsync(quantidade);

                // Filtrar por usuário se não for admin
                if (!isAdmin)
                {
                    deploys = deploys.Where(d => d.UsuarioId == currentUserId);
                }

                var response = deploys.Select(d => new DeployResumoResponse
                {
                    Id = d.Id,
                    SiteName = d.SiteName,
                    ApplicationName = d.ApplicationName,
                    Data = d.Data,
                    Status = d.Status,
                    UsuarioNome = d.Usuario.Nome,
                    Plataforma = d.Plataforma
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter deploys recentes");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obtém os comandos de um deploy específico com seus status
        /// </summary>
        [HttpGet("{id}/comandos")]
        public async Task<ActionResult<IEnumerable<DeployComandoResponse>>> GetComandosDeploy(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                var deploy = await _deployService.ObterDeployPorIdAsync(id);
                if (deploy == null)
                {
                    return NotFound("Deploy não encontrado");
                }

                // Verificar permissão
                if (!isAdmin && deploy.UsuarioId != currentUserId)
                {
                    return Forbid("Você só pode visualizar seus próprios deploys");
                }

                var comandos = await _deployService.ObterComandosDeployAsync(id);
                var response = comandos.Select(dc => new DeployComandoResponse
                {
                    Id = dc.Id,
                    Comando = dc.Comando,
                    Ordem = dc.Ordem,
                    CriadoEm = dc.CriadoEm,
                    Status = dc.Status,
                    Mensagem = dc.Mensagem,
                    ExecutadoEm = dc.ExecutadoEm,
                    TerminalId = dc.TerminalId // Adicionado o campo TerminalId
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter comandos do deploy {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Reexecuta um comando específico de um deploy
        /// </summary>
        [HttpPost("{id}/comandos/{comandoId}/reexecutar")]
        public async Task<ActionResult> ReexecutarComando(int id, int comandoId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAdmin = await _usuarioService.IsAdministradorAsync(currentUserId);

                var deploy = await _deployService.ObterDeployPorIdAsync(id);
                if (deploy == null)
                {
                    return NotFound("Deploy não encontrado");
                }

                // Verificar permissão
                if (!isAdmin && deploy.UsuarioId != currentUserId)
                {
                    return Forbid("Você só pode reexecutar comandos de seus próprios deploys");
                }

                // Buscar o comando específico
                var comandos = await _deployService.ObterComandosDeployAsync(id);
                var comando = comandos.FirstOrDefault(c => c.Id == comandoId);
                
                if (comando == null)
                {
                    return NotFound("Comando não encontrado");
                }

                // Aqui poderia implementar a lógica de reexecução
                // Por enquanto, vamos apenas atualizar o status para "Pendente"
                // TODO: Implementar lógica de reexecução real

                return Ok(new { message = "Funcionalidade de reexecução será implementada em breve" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reexecutar comando {ComandoId} do deploy {Id}", comandoId, id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Exclui um deploy
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ExcluirDeploy(int id)
        {
            try
            {
                var resultado = await _deployService.ExcluirDeployAsync(id);
                if (!resultado)
                {
                    return NotFound("Deploy não encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir deploy {Id}", id);
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
