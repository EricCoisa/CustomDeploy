using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomDeploy.Models;
using CustomDeploy.Services;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("api/iis")]
    [Authorize]
    public class IISController : ControllerBase
    {
        private readonly ILogger<IISController> _logger;
        private readonly IISManagementService _iisManagementService;
        private readonly AdministratorService _administratorService;

        public IISController(ILogger<IISController> logger, IISManagementService iisManagementService, AdministratorService administratorService)
        {
            _logger = logger;
            _iisManagementService = iisManagementService;
            _administratorService = administratorService;
        }

        /// <summary>
        /// Verifica se a aplicação possui permissões para gerenciar IIS
        /// </summary>
        /// <returns>Status das permissões necessárias</returns>
        [HttpPost("verify-permissions")]
        public async Task<IActionResult> VerifyPermissions()
        {
            try
            {
                _logger.LogInformation("Solicitação para verificar permissões IIS recebida");

                var result = await _iisManagementService.VerifyPermissionsAsync();

                if (result.Success)
                {
                    var response = new
                    {
                        message = result.Message,
                        permissions = result.Permissions,
                        timestamp = DateTime.UtcNow
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        message = result.Message,
                        permissions = result.Permissions,
                        timestamp = DateTime.UtcNow
                    };

                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar permissões IIS");
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao verificar permissões", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Verifica se está executando como administrador e retorna instruções se necessário
        /// </summary>
        /// <returns>Status de privilégios e instruções</returns>
        [HttpGet("admin-status")]
        public IActionResult GetAdministratorStatus()
        {
            try
            {
                _logger.LogInformation("Solicitação para verificar status de administrador");

                var (isAdmin, userName, domain, instructions) = _administratorService.GetPrivilegeStatus();

                var response = new
                {
                    isAdministrator = isAdmin,
                    currentUser = new
                    {
                        name = userName,
                        domain = domain,
                        fullName = $"{domain}\\{userName}"
                    },
                    canManageIIS = isAdmin,
                    instructions = instructions,
                    message = isAdmin 
                        ? "Aplicação executando com privilégios de administrador" 
                        : "Privilégios de administrador são necessários para gerenciar IIS",
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status de administrador");
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao verificar privilégios", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Tenta reiniciar a aplicação como administrador
        /// </summary>
        /// <returns>Resultado da tentativa de reinicialização</returns>
        [HttpPost("request-admin")]
        public IActionResult RequestAdministratorPrivileges()
        {
            try
            {
                _logger.LogInformation("Solicitação para reiniciar como administrador");

                // Verificar se já é administrador
                if (_administratorService.IsRunningAsAdministrator())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "A aplicação já está executando como administrador",
                        alreadyAdmin = true,
                        timestamp = DateTime.UtcNow
                    });
                }

                var (success, message) = _administratorService.RequestAdministratorRestart();

                var response = new
                {
                    success = success,
                    message = message,
                    instructions = success 
                        ? new List<string> { "✅ Nova instância sendo iniciada como administrador", "Esta instância será fechada automaticamente" }
                        : _administratorService.GetAdministratorInstructions(),
                    timestamp = DateTime.UtcNow
                };

                if (success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar reiniciar como administrador");
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "Erro interno do servidor ao tentar reiniciar como administrador", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Verifica permissões e retorna instruções detalhadas para resolver problemas
        /// </summary>
        /// <returns>Resultado detalhado da verificação com instruções</returns>
        [HttpGet("request-permissions")]
        public async Task<IActionResult> RequestPermissions()
        {
            try
            {
                _logger.LogInformation("Solicitação para verificar permissões com instruções recebida");

                var result = await _iisManagementService.RequestPermissionsAsync();

                var response = new
                {
                    success = result.AllPermissionsGranted,
                    message = result.AllPermissionsGranted 
                        ? "Todas as permissões necessárias estão disponíveis" 
                        : "Algumas permissões estão faltando",
                    permissions = new
                    {
                        canCreateFolders = result.CanCreateFolders,
                        canMoveFiles = result.CanMoveFiles,
                        canExecuteIISCommands = result.CanExecuteIISCommands,
                        allPermissionsGranted = result.AllPermissionsGranted
                    },
                    testDetails = result.TestDetails,
                    instructions = result.Instructions,
                    timestamp = DateTime.UtcNow
                };

                if (result.AllPermissionsGranted)
                {
                    return Ok(response);
                }
                else
                {
                    // Retorna 200 mas indica que há problemas de permissão
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar permissões com instruções");
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "Erro interno do servidor ao verificar permissões", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Cria um novo site no IIS
        /// </summary>
        /// <param name="request">Dados do site a ser criado</param>
        /// <returns>Resultado da criação do site</returns>
        [HttpPost("create-site")]
        public async Task<IActionResult> CreateSite([FromBody] CreateSiteRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitação para criar site IIS: {SiteName}", request?.SiteName);

                if (request == null)
                {
                    return BadRequest(new 
                    { 
                        message = "Dados do site são obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Validações básicas
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(request.SiteName))
                {
                    validationErrors.Add("Nome do site é obrigatório");
                }

                if (request.Port <= 0 || request.Port > 65535)
                {
                    validationErrors.Add("Porta deve estar entre 1 e 65535");
                }

                if (string.IsNullOrWhiteSpace(request.PhysicalPath))
                {
                    validationErrors.Add("Caminho físico é obrigatório");
                }

                if (string.IsNullOrWhiteSpace(request.AppPool))
                {
                    validationErrors.Add("Nome do Application Pool é obrigatório");
                }

                if (validationErrors.Any())
                {
                    return BadRequest(new 
                    { 
                        message = "Dados inválidos",
                        errors = validationErrors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var result = await _iisManagementService.CreateSiteAsync(
                    request.SiteName, 
                    request.Port, 
                    request.PhysicalPath, 
                    request.AppPool);

                if (result.Success)
                {
                    var response = new
                    {
                        message = result.Message,
                        site = result.Details,
                        timestamp = DateTime.UtcNow
                    };

                    return CreatedAtAction(nameof(CreateSite), response);
                }
                else
                {
                    var response = new
                    {
                        message = result.Message,
                        details = result.Details,
                        timestamp = DateTime.UtcNow
                    };

                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar site IIS: {SiteName}", request?.SiteName);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao criar site", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Lista todos os sites do IIS (endpoint adicional para conveniência)
        /// </summary>
        /// <returns>Lista de sites existentes</returns>
        [HttpGet("sites")]
        public async Task<IActionResult> GetSites()
        {
            try
            {
                _logger.LogInformation("Solicitação para listar sites IIS");

                var result = await _iisManagementService.GetAllSitesAsync();
                
                if (result.Success)
                {
                    var response = new
                    {
                        message = result.Message,
                        sites = result.Sites,
                        timestamp = DateTime.UtcNow
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        message = result.Message,
                        error = "Failed to retrieve sites",
                        timestamp = DateTime.UtcNow
                    };

                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar sites IIS");
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao listar sites", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Lista todos os Application Pools do IIS (endpoint adicional para conveniência)
        /// </summary>
        /// <returns>Lista de Application Pools existentes</returns>
        [HttpGet("app-pools")]
        public async Task<IActionResult> GetAppPools()
        {
            try
            {
                _logger.LogInformation("Solicitação para listar Application Pools IIS");

                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Get-IISAppPool | ConvertTo-Json\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new System.Diagnostics.Process { StartInfo = processInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    var response = new
                    {
                        message = "Application Pools listados com sucesso",
                        appPools = string.IsNullOrWhiteSpace(output) ? "Nenhum Application Pool encontrado" : output,
                        timestamp = DateTime.UtcNow
                    };

                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        message = "Erro ao listar Application Pools",
                        error = error,
                        timestamp = DateTime.UtcNow
                    };

                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar Application Pools IIS");
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao listar Application Pools", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Obtém informações detalhadas de um site específico
        /// </summary>
        /// <param name="siteName">Nome do site</param>
        /// <returns>Informações detalhadas do site</returns>
        [HttpGet("sites/{siteName}")]
        public async Task<IActionResult> GetSiteInfo(string siteName)
        {
            try
            {
                _logger.LogInformation("Solicitação para obter informações do site: {SiteName}", siteName);

                var result = await _iisManagementService.GetSiteInfoAsync(siteName);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        siteInfo = result.SiteInfo,
                        physicalPath = result.PhysicalPath,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        message = result.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do site: {SiteName}", siteName);
                return StatusCode(500, new { message = "Erro interno", details = ex.Message });
            }
        }

        /// <summary>
        /// Verifica se uma aplicação existe em um site específico
        /// </summary>
        /// <param name="siteName">Nome do site</param>
        /// <param name="applicationPath">Caminho da aplicação</param>
        /// <returns>Status da aplicação</returns>
        [HttpGet("sites/{siteName}/applications/{applicationPath}")]
        public async Task<IActionResult> CheckApplication(string siteName, string applicationPath)
        {
            try
            {
                _logger.LogInformation("Verificando aplicação: {SiteName}/{ApplicationPath}", siteName, applicationPath);

                var result = await _iisManagementService.CheckApplicationExistsAsync(siteName, applicationPath);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        applicationExists = result.ApplicationExists,
                        applicationInfo = result.ApplicationInfo,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        message = result.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar aplicação: {SiteName}/{ApplicationPath}", siteName, applicationPath);
                return StatusCode(500, new { message = "Erro interno", details = ex.Message });
            }
        }
    }
}
