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
    [Route("[controller]")]
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
                    
                    return StatusCode(403, new
                    {
                        message = "Acesso negado: privilégios de administrador são necessários para navegar no sistema de arquivos",
                        timestamp = DateTime.UtcNow
                    });
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
                    return StatusCode(403, new
                    {
                        message = "Acesso negado: privilégios de administrador são necessários",
                        timestamp = DateTime.UtcNow
                    });
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
                    return StatusCode(403, new
                    {
                        message = "Acesso negado: privilégios de administrador são necessários",
                        timestamp = DateTime.UtcNow
                    });
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
                    return StatusCode(403, new
                    {
                        message = "Acesso negado: privilégios de administrador são necessários",
                        timestamp = DateTime.UtcNow
                    });
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
                    return StatusCode(403, new
                    {
                        message = "Acesso negado: privilégios de administrador são necessários",
                        timestamp = DateTime.UtcNow
                    });
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

        /// <summary>
        /// Cria uma nova pasta
        /// </summary>
        /// <param name="request">Dados para criação da pasta</param>
        /// <returns>Resultado da operação</returns>
        [HttpPost("create-directory")]
        public async Task<IActionResult> CreateDirectory([FromBody] CreateDirectoryRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Path))
                {
                    return BadRequest(new
                    {
                        message = "Caminho da pasta é obrigatório",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Solicitação de criação de pasta: {Path}", request.Path);

                // Verifica privilégios de administrador
                var adminStatus = _administratorService.GetPrivilegeStatus();
                if (!adminStatus.IsAdmin)
                {
                    return StatusCode(403, new
                    {
                        message = "Acesso negado: privilégios de administrador são necessários",
                        timestamp = DateTime.UtcNow
                    });
                }

                var success = await _fileManagerService.CreateDirectoryAsync(request.Path);

                if (success)
                {
                    return Ok(new
                    {
                        message = "Pasta criada com sucesso",
                        path = request.Path,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Falha ao criar pasta. Verifique se o caminho é válido e se a pasta não existe",
                        path = request.Path,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pasta: {Path}", request?.Path);
                
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Renomeia uma pasta existente
        /// </summary>
        /// <param name="request">Dados para renomeação da pasta</param>
        /// <returns>Resultado da operação</returns>
        [HttpPut("rename-directory")]
        public async Task<IActionResult> RenameDirectory([FromBody] RenameDirectoryRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.OldPath) || string.IsNullOrWhiteSpace(request.NewName))
                {
                    return BadRequest(new
                    {
                        message = "Caminho atual e novo nome são obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Solicitação de renomeação de pasta: {OldPath} -> {NewName}", request.OldPath, request.NewName);

                // Verifica privilégios de administrador
                var adminStatus = _administratorService.GetPrivilegeStatus();
                if (!adminStatus.IsAdmin)
                {
                    return StatusCode(403, new
                    {
                        message = "Acesso negado: privilégios de administrador são necessários",
                        timestamp = DateTime.UtcNow
                    });
                }

                var success = await _fileManagerService.RenameDirectoryAsync(request.OldPath, request.NewName);

                if (success)
                {
                    var parentPath = Path.GetDirectoryName(request.OldPath);
                    var newPath = Path.Combine(parentPath!, request.NewName);
                    
                    return Ok(new
                    {
                        message = "Pasta renomeada com sucesso",
                        oldPath = request.OldPath,
                        newPath = newPath,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Falha ao renomear pasta. Verifique se a pasta existe e o novo nome é válido",
                        oldPath = request.OldPath,
                        newName = request.NewName,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renomear pasta: {OldPath} -> {NewName}", request?.OldPath, request?.NewName);
                
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Deleta uma pasta
        /// </summary>
        /// <param name="request">Dados para deleção da pasta</param>
        /// <returns>Resultado da operação</returns>
        [HttpDelete("delete-directory")]
        public async Task<IActionResult> DeleteDirectory([FromBody] DeleteDirectoryRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Path))
                {
                    return BadRequest(new
                    {
                        message = "Caminho da pasta é obrigatório",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Solicitação de deleção de pasta: {Path}, Recursive: {Recursive}", request.Path, request.Recursive);

                // Verifica privilégios de administrador
                var adminStatus = _administratorService.GetPrivilegeStatus();
                if (!adminStatus.IsAdmin)
                {
                    return StatusCode(403, new
                    {
                        message = "Acesso negado: privilégios de administrador são necessários",
                        timestamp = DateTime.UtcNow
                    });
                }

                var success = await _fileManagerService.DeleteDirectoryAsync(request.Path, request.Recursive);

                if (success)
                {
                    return Ok(new
                    {
                        message = "Pasta deletada com sucesso",
                        path = request.Path,
                        recursive = request.Recursive,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Falha ao deletar pasta. Verifique se a pasta existe e está acessível",
                        path = request.Path,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar pasta: {Path}", request?.Path);
                
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
