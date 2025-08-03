using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomDeploy.Models;
using CustomDeploy.Services;

namespace CustomDeploy.Controllers
{
    /// <summary>
    /// Controller para gerenciamento e navegação do sistema de arquivos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requer autenticação JWT
    public class FileManagerController : ControllerBase
    {
        private readonly IFileManagerService _fileManagerService;
        private readonly AdministratorService _administratorService;
        private readonly ILogger<FileManagerController> _logger;

        public FileManagerController(
            IFileManagerService fileManagerService,
            AdministratorService administratorService,
            ILogger<FileManagerController> logger)
        {
            _fileManagerService = fileManagerService;
            _administratorService = administratorService;
            _logger = logger;
        }

        /// <summary>
        /// Navega pelo sistema de arquivos e retorna o conteúdo de um diretório
        /// </summary>
        /// <param name="path">Caminho do diretório (opcional, padrão é C:\)</param>
        /// <param name="includeHidden">Incluir arquivos ocultos (padrão: false)</param>
        /// <param name="fileExtensionFilter">Filtro por extensão de arquivo (ex: .txt)</param>
        /// <param name="sortBy">Campo para ordenação: name, size, date, type (padrão: name)</param>
        /// <param name="ascending">Ordem crescente (padrão: true)</param>
        /// <returns>Conteúdo do diretório com arquivos e pastas</returns>
        [HttpGet]
        public async Task<IActionResult> Browse(
            [FromQuery] string? path = null,
            [FromQuery] bool includeHidden = false,
            [FromQuery] string? fileExtensionFilter = null,
            [FromQuery] string sortBy = "name",
            [FromQuery] bool ascending = true)
        {
            try
            {
                _logger.LogInformation("Solicitação de navegação no sistema de arquivos. Path: {Path}, User: {User}", 
                    path ?? "C:\\", User.Identity?.Name);

                // Verifica se o usuário é administrador
                var adminStatus = _administratorService.GetPrivilegeStatus();
                if (!adminStatus.IsAdmin)
                {
                    _logger.LogWarning("Usuário {User} tentou acessar o sistema de arquivos sem privilégios de administrador", 
                        User.Identity?.Name);
                    
                    return Forbid("Acesso negado: privilégios de administrador são necessários para navegar no sistema de arquivos");
                }

                // Executa a navegação
                var result = await _fileManagerService.GetDirectoryContentsAsync(
                    path, includeHidden, fileExtensionFilter, sortBy, ascending);

                if (!result.IsAccessible)
                {
                    return BadRequest(new
                    {
                        message = result.ErrorMessage ?? "Erro ao acessar o diretório",
                        path = result.CurrentPath,
                        timestamp = result.Timestamp
                    });
                }

                _logger.LogInformation("Navegação concluída com sucesso. Path: {Path}, Items: {ItemCount}", 
                    result.CurrentPath, result.TotalItems);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao navegar no sistema de arquivos. Path: {Path}", path);
                
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor ao navegar no sistema de arquivos",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Obtém informações detalhadas de um item específico do sistema de arquivos
        /// </summary>
        /// <param name="path">Caminho completo do item</param>
        /// <returns>Informações detalhadas do item</returns>
        [HttpGet("item")]
        public async Task<IActionResult> GetItemInfo([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return BadRequest(new
                    {
                        message = "Caminho é obrigatório",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Solicitação de informações do item: {Path}", path);

                // Verifica privilégios de administrador
                var adminStatus = _administratorService.GetPrivilegeStatus();
                if (!adminStatus.IsAdmin)
                {
                    return Forbid("Acesso negado: privilégios de administrador são necessários");
                }

                var item = await _fileManagerService.GetItemInfoAsync(path);
                
                if (item == null)
                {
                    return NotFound(new
                    {
                        message = "Item não encontrado ou inacessível",
                        path = path,
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    message = "Informações do item obtidas com sucesso",
                    item = item,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do item: {Path}", path);
                
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Obtém a lista de unidades de disco disponíveis no sistema
        /// </summary>
        /// <returns>Lista de drives disponíveis</returns>
        [HttpGet("drives")]
        public async Task<IActionResult> GetAvailableDrives()
        {
            try
            {
                _logger.LogInformation("Solicitação de unidades de disco disponíveis");

                // Verifica privilégios de administrador
                var adminStatus = _administratorService.GetPrivilegeStatus();
                if (!adminStatus.IsAdmin)
                {
                    return Forbid("Acesso negado: privilégios de administrador são necessários");
                }

                var drives = _fileManagerService.GetAvailableDrives();

                return Ok(new
                {
                    message = $"Encontradas {drives.Count} unidades de disco",
                    drives = drives,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter unidades de disco disponíveis");
                
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Verifica se um caminho é válido e acessível
        /// </summary>
        /// <param name="path">Caminho para verificar</param>
        /// <returns>Status de validade e acessibilidade do caminho</returns>
        [HttpGet("validate")]
        public async Task<IActionResult> ValidatePath([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return BadRequest(new
                    {
                        message = "Caminho é obrigatório",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Validação de caminho solicitada: {Path}", path);

                // Verifica privilégios de administrador
                var adminStatus = _administratorService.GetPrivilegeStatus();
                if (!adminStatus.IsAdmin)
                {
                    return Forbid("Acesso negado: privilégios de administrador são necessários");
                }

                var isValid = _fileManagerService.IsPathValidAndAccessible(path);
                var isBlocked = _fileManagerService.IsPathBlocked(path);

                return Ok(new
                {
                    path = path,
                    isValid = isValid,
                    isBlocked = isBlocked,
                    isAccessible = isValid && !isBlocked,
                    message = GetValidationMessage(isValid, isBlocked),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar caminho: {Path}", path);
                
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Endpoint para obter informações de status e capacidades do sistema de arquivos
        /// </summary>
        /// <returns>Informações do sistema</returns>
        [HttpGet("system-info")]
        public async Task<IActionResult> GetSystemInfo()
        {
            try
            {
                _logger.LogInformation("Solicitação de informações do sistema");

                // Verifica privilégios de administrador
                var adminStatus = _administratorService.GetPrivilegeStatus();
                if (!adminStatus.IsAdmin)
                {
                    return Forbid("Acesso negado: privilégios de administrador são necessários");
                }

                var drives = _fileManagerService.GetAvailableDrives();
                var systemInfo = new
                {
                    operatingSystem = Environment.OSVersion.ToString(),
                    machineName = Environment.MachineName,
                    userName = Environment.UserName,
                    currentDirectory = Environment.CurrentDirectory,
                    systemDirectory = Environment.SystemDirectory,
                    availableDrives = drives.Count,
                    drives = drives.Select(d => new
                    {
                        d.Name,
                        d.FullPath,
                        d.IsAccessible,
                        SizeGB = d.Size.HasValue ? Math.Round(d.Size.Value / (1024.0 * 1024.0 * 1024.0), 2) : (double?)null
                    }),
                    timestamp = DateTime.UtcNow
                };

                return Ok(new
                {
                    message = "Informações do sistema obtidas com sucesso",
                    systemInfo = systemInfo,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do sistema");
                
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        private string GetValidationMessage(bool isValid, bool isBlocked)
        {
            if (!isValid)
                return "Caminho inválido ou não encontrado";
            
            if (isBlocked)
                return "Caminho válido, mas bloqueado por política de segurança";
            
            return "Caminho válido e acessível";
        }
    }
}
