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

        #region Permission Management

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
                        canManageIIS = result.CanManageIIS,
                        allPermissionsGranted = result.AllPermissionsGranted
                    },
                    testDetails = result.TestDetails,
                    instructions = result.Instructions,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
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

        #endregion

        #region Sites CRUD

        /// <summary>
        /// Cria um novo site no IIS
        /// </summary>
        /// <param name="request">Dados do site a ser criado</param>
        /// <returns>Resultado da criação do site</returns>
        [HttpPost("sites")]
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
                    validationErrors.Add("Nome do site é obrigatório");

                if (string.IsNullOrWhiteSpace(request.BindingInformation))
                    validationErrors.Add("Binding information é obrigatório");

                if (string.IsNullOrWhiteSpace(request.PhysicalPath))
                    validationErrors.Add("Caminho físico é obrigatório");

                if (string.IsNullOrWhiteSpace(request.AppPoolName))
                    validationErrors.Add("Nome do Application Pool é obrigatório");

                if (validationErrors.Any())
                {
                    return BadRequest(new 
                    { 
                        message = "Dados inválidos",
                        errors = validationErrors,
                        timestamp = DateTime.UtcNow
                    });
                }

                var result = await _iisManagementService.CreateSiteAsync(request);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetSiteInfo), new { siteName = request.SiteName }, new
                    {
                        message = result.Message,
                        site = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
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
        /// Lista todos os sites do IIS com informações detalhadas
        /// </summary>
        /// <returns>Lista de sites existentes</returns>
        [HttpGet("sites")]
        public async Task<IActionResult> GetSites()
        {
            try
            {
                _logger.LogInformation("Solicitação para listar sites IIS");

                var result = await _iisManagementService.GetAllDetailedSitesAsync();
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        sites = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
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

                var siteInfo = await _iisManagementService.GetDetailedSiteInfoAsync(siteName);

                if (siteInfo != null)
                {
                    return Ok(new
                    {
                        message = $"Informações do site '{siteName}' obtidas com sucesso",
                        site = siteInfo,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        message = $"Site '{siteName}' não encontrado",
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
        /// Atualiza um site existente no IIS
        /// </summary>
        /// <param name="siteName">Nome do site a ser atualizado</param>
        /// <param name="request">Dados de atualização</param>
        /// <returns>Resultado da atualização</returns>
        [HttpPut("sites/{siteName}")]
        public async Task<IActionResult> UpdateSite(string siteName, [FromBody] UpdateSiteRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitação para atualizar site IIS: {SiteName}", siteName);

                if (request == null)
                {
                    return BadRequest(new 
                    { 
                        message = "Dados de atualização são obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                var result = await _iisManagementService.UpdateSiteAsync(siteName, request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        site = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar site IIS: {SiteName}", siteName);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao atualizar site", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Remove um site do IIS
        /// </summary>
        /// <param name="siteName">Nome do site a ser removido</param>
        /// <returns>Resultado da remoção</returns>
        [HttpDelete("sites/{siteName}")]
        public async Task<IActionResult> DeleteSite(string siteName)
        {
            try
            {
                _logger.LogInformation("Solicitação para remover site IIS: {SiteName}", siteName);

                var result = await _iisManagementService.DeleteSiteAsync(siteName);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover site IIS: {SiteName}", siteName);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao remover site", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        #endregion

        #region Applications CRUD

        /// <summary>
        /// Cria uma nova aplicação em um site existente
        /// </summary>
        /// <param name="request">Dados da aplicação a ser criada</param>
        /// <returns>Resultado da criação da aplicação</returns>
        [HttpPost("applications")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitação para criar aplicação: {SiteName}/{AppPath}", request?.SiteName, request?.AppPath);

                if (request == null)
                {
                    return BadRequest(new 
                    { 
                        message = "Dados da aplicação são obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Validações básicas
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(request.SiteName))
                    validationErrors.Add("Nome do site é obrigatório");

                if (string.IsNullOrWhiteSpace(request.AppPath))
                    validationErrors.Add("Caminho da aplicação é obrigatório");

                if (string.IsNullOrWhiteSpace(request.PhysicalPath))
                    validationErrors.Add("Caminho físico é obrigatório");

                if (string.IsNullOrWhiteSpace(request.AppPoolName))
                    validationErrors.Add("Nome do Application Pool é obrigatório");

                // Garantir que o AppPath comece com /
                if (!string.IsNullOrWhiteSpace(request.AppPath) && !request.AppPath.StartsWith("/"))
                {
                    request.AppPath = "/" + request.AppPath;
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

                var result = await _iisManagementService.CreateApplicationAsync(request);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetSiteApplications), new { siteName = request.SiteName }, new
                    {
                        message = result.Message,
                        application = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar aplicação: {SiteName}/{AppPath}", request?.SiteName, request?.AppPath);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao criar aplicação", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Lista todas as aplicações de um site específico
        /// </summary>
        /// <param name="siteName">Nome do site</param>
        /// <returns>Lista de aplicações do site</returns>
        [HttpGet("sites/{siteName}/applications")]
        public async Task<IActionResult> GetSiteApplications(string siteName)
        {
            try
            {
                _logger.LogInformation("Solicitação para listar aplicações do site: {SiteName}", siteName);

                var result = await _iisManagementService.GetSiteApplicationsAsync(siteName);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        applications = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar aplicações do site: {SiteName}", siteName);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao listar aplicações", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Atualiza uma aplicação existente
        /// </summary>
        /// <param name="siteName">Nome do site</param>
        /// <param name="appPath">Caminho da aplicação</param>
        /// <param name="request">Dados de atualização</param>
        /// <returns>Resultado da atualização</returns>
        [HttpPut("sites/{siteName}/applications/{*appPath}")]
        public async Task<IActionResult> UpdateApplication(string siteName, string appPath, [FromBody] UpdateApplicationRequest request)
        {
            try
            {
                // Garantir que o appPath comece com /
                if (!appPath.StartsWith("/"))
                {
                    appPath = "/" + appPath;
                }

                _logger.LogInformation("Solicitação para atualizar aplicação: {SiteName}/{AppPath}", siteName, appPath);

                if (request == null)
                {
                    return BadRequest(new 
                    { 
                        message = "Dados de atualização são obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                var result = await _iisManagementService.UpdateApplicationAsync(siteName, appPath, request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        application = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar aplicação: {SiteName}/{AppPath}", siteName, appPath);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao atualizar aplicação", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Remove uma aplicação de um site
        /// </summary>
        /// <param name="siteName">Nome do site</param>
        /// <param name="appPath">Caminho da aplicação</param>
        /// <returns>Resultado da remoção</returns>
        [HttpDelete("sites/{siteName}/applications/{*appPath}")]
        public async Task<IActionResult> DeleteApplication(string siteName, string appPath)
        {
            try
            {
                // Garantir que o appPath comece com /
                if (!appPath.StartsWith("/"))
                {
                    appPath = "/" + appPath;
                }

                _logger.LogInformation("Solicitação para remover aplicação: {SiteName}/{AppPath}", siteName, appPath);

                var result = await _iisManagementService.DeleteApplicationAsync(siteName, appPath);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover aplicação: {SiteName}/{AppPath}", siteName, appPath);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao remover aplicação", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Verifica se uma aplicação existe em um site específico
        /// </summary>
        /// <param name="siteName">Nome do site</param>
        /// <param name="appPath">Caminho da aplicação</param>
        /// <returns>Status da aplicação</returns>
        [HttpGet("sites/{siteName}/applications/check/{*appPath}")]
        public async Task<IActionResult> CheckApplication(string siteName, string appPath)
        {
            try
            {
                // Garantir que o appPath comece com /
                if (!appPath.StartsWith("/"))
                {
                    appPath = "/" + appPath;
                }

                _logger.LogInformation("Verificando aplicação: {SiteName}/{AppPath}", siteName, appPath);

                var result = await _iisManagementService.CheckApplicationExistsAsync(siteName, appPath);

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
                _logger.LogError(ex, "Erro ao verificar aplicação: {SiteName}/{AppPath}", siteName, appPath);
                return StatusCode(500, new { message = "Erro interno", details = ex.Message });
            }
        }

        #endregion

        #region Application Pools CRUD

        /// <summary>
        /// Cria um novo Application Pool
        /// </summary>
        /// <param name="request">Dados do Application Pool a ser criado</param>
        /// <returns>Resultado da criação do Application Pool</returns>
        [HttpPost("app-pools")]
        public async Task<IActionResult> CreateAppPool([FromBody] CreateAppPoolRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitação para criar Application Pool: {PoolName}", request?.PoolName);

                if (request == null)
                {
                    return BadRequest(new 
                    { 
                        message = "Dados do Application Pool são obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Validações básicas
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(request.PoolName))
                    validationErrors.Add("Nome do Application Pool é obrigatório");

                if (!string.IsNullOrWhiteSpace(request.PipelineMode) && 
                    !request.PipelineMode.Equals("Classic", StringComparison.OrdinalIgnoreCase) &&
                    !request.PipelineMode.Equals("Integrated", StringComparison.OrdinalIgnoreCase))
                {
                    validationErrors.Add("Pipeline Mode deve ser 'Classic' ou 'Integrated'");
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

                var result = await _iisManagementService.CreateAppPoolAsync(request);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetAppPools), new
                    {
                        message = result.Message,
                        appPool = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar Application Pool: {PoolName}", request?.PoolName);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao criar Application Pool", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Lista todos os Application Pools do IIS
        /// </summary>
        /// <returns>Lista de Application Pools existentes</returns>
        [HttpGet("app-pools")]
        public async Task<IActionResult> GetAppPools()
        {
            try
            {
                _logger.LogInformation("Solicitação para listar Application Pools IIS");

                var result = await _iisManagementService.GetAllAppPoolsAsync();

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        appPools = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
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
        /// Atualiza um Application Pool existente
        /// </summary>
        /// <param name="poolName">Nome do Application Pool</param>
        /// <param name="request">Dados de atualização</param>
        /// <returns>Resultado da atualização</returns>
        [HttpPut("app-pools/{poolName}")]
        public async Task<IActionResult> UpdateAppPool(string poolName, [FromBody] UpdateAppPoolRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitação para atualizar Application Pool: {PoolName}", poolName);

                if (request == null)
                {
                    return BadRequest(new 
                    { 
                        message = "Dados de atualização são obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Validar Pipeline Mode se fornecido
                if (!string.IsNullOrWhiteSpace(request.PipelineMode) && 
                    !request.PipelineMode.Equals("Classic", StringComparison.OrdinalIgnoreCase) &&
                    !request.PipelineMode.Equals("Integrated", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new 
                    { 
                        message = "Pipeline Mode deve ser 'Classic' ou 'Integrated'",
                        timestamp = DateTime.UtcNow
                    });
                }

                var result = await _iisManagementService.UpdateAppPoolAsync(poolName, request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        appPool = result.Data,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar Application Pool: {PoolName}", poolName);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao atualizar Application Pool", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Remove um Application Pool (apenas se não estiver em uso)
        /// </summary>
        /// <param name="poolName">Nome do Application Pool</param>
        /// <returns>Resultado da remoção</returns>
        [HttpDelete("app-pools/{poolName}")]
        public async Task<IActionResult> DeleteAppPool(string poolName)
        {
            try
            {
                _logger.LogInformation("Solicitação para remover Application Pool: {PoolName}", poolName);

                var result = await _iisManagementService.DeleteAppPoolAsync(poolName);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message,
                        errors = result.Errors,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover Application Pool: {PoolName}", poolName);
                return StatusCode(500, new 
                { 
                    message = "Erro interno do servidor ao remover Application Pool", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        #endregion
    }
}
