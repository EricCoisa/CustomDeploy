using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomDeploy.Models;
using CustomDeploy.Services;
using Microsoft.Extensions.Options;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("system")]
    [Authorize]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;
        private readonly DeployService _deployService;
        private readonly GitHubService _gitHubService;
        private readonly GitHubSettings _gitHubSettings;

        public SystemController(
            ILogger<SystemController> logger, 
            DeployService deployService,
            GitHubService gitHubService,
            IOptions<GitHubSettings> gitHubSettings)
        {
            _logger = logger;
            _deployService = deployService;
            _gitHubService = gitHubService;
            _gitHubSettings = gitHubSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Deploy([FromBody] DeployRequest request)
        {
            try
            {
                _logger.LogInformation("Deploy request received for repository: {RepoUrl}, Branch: {Branch}", 
                    request.RepoUrl, request.Branch);

                // Validação básica
                if (string.IsNullOrWhiteSpace(request.RepoUrl) ||
                    string.IsNullOrWhiteSpace(request.Branch) ||
                    request.BuildCommand == null || request.BuildCommand.Length == 0 ||
                    string.IsNullOrWhiteSpace(request.BuildOutput) ||
                    string.IsNullOrWhiteSpace(request.IisSiteName))
                {
                    return BadRequest(new { message = "RepoUrl, Branch, BuildCommands (at least one), BuildOutput and IisSiteName are required" });
                }

                // TargetPath é opcional quando ApplicationPath é especificado
                // Ambos podem ser opcionais para deploy no root do site

                // Executar o deploy usando o DeployService
                var result = await _deployService.ExecuteDeployAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        repository = request.RepoUrl,
                        branch = request.Branch,
                        buildCommands = request.BuildCommand,
                        buildOutput = request.BuildOutput,
                        targetPath = request.TargetPath,
                        iisSiteName = request.IisSiteName,
                        timestamp = DateTime.UtcNow,
                        deployDetails = result.DeployDetails
                    });
                }
                else
                {
                    _logger.LogError("Deploy failed: {ErrorMessage}", result.Message);
                    return StatusCode(500, new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing deploy request");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Verifica o status das credenciais Git configuradas
        /// </summary>
        /// <returns>Status das credenciais do sistema e configuradas</returns>
        [HttpGet("credentials/status")]
        public async Task<IActionResult> GetCredentialsStatus()
        {
            try
            {
                _logger.LogInformation("Verificando status das credenciais Git");

                var status = new
                {
                    configuredCredentials = new
                    {
                        hasCredentials = _gitHubSettings.HasCredentials,
                        username = _gitHubSettings.HasCredentials ? _gitHubSettings.Username : null,
                        hasToken = _gitHubSettings.HasCredentials && !string.IsNullOrWhiteSpace(_gitHubSettings.PersonalAccessToken),
                        useSystemCredentials = _gitHubSettings.UseSystemCredentials,
                        apiBaseUrl = _gitHubSettings.ApiBaseUrl,
                        gitTimeoutSeconds = _gitHubSettings.GitTimeoutSeconds
                    },
                    systemCredentials = await CheckSystemGitCredentialsAsync(),
                    timestamp = DateTime.UtcNow
                };

                return Ok(new
                {
                    message = "Status das credenciais obtido com sucesso",
                    status = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status das credenciais");
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Testa as credenciais Git com um repositório específico
        /// </summary>
        /// <param name="testRepoUrl">URL do repositório para teste (opcional, usa um repositório público por padrão)</param>
        /// <returns>Resultado do teste de credenciais</returns>
        [HttpPost("credentials/test")]
        public async Task<IActionResult> TestCredentials([FromBody] TestCredentialsRequest? request = null)
        {
            try
            {
                var testRepoUrl = request?.RepoUrl ?? "https://github.com/octocat/Hello-World.git";
                
                _logger.LogInformation("Testando credenciais com repositório: {RepoUrl}", testRepoUrl);

                var testResults = new
                {
                    testRepository = testRepoUrl,
                    systemCredentials = await TestSystemCredentialsAsync(testRepoUrl),
                    configuredCredentials = await TestConfiguredCredentialsAsync(testRepoUrl),
                    overallStatus = "pending" as string,
                    recommendation = "pending" as string,
                    timestamp = DateTime.UtcNow
                };

                // Determinar status geral e recomendação
                var (overallStatus, recommendation) = DetermineOverallCredentialsStatus(
                    testResults.systemCredentials, 
                    testResults.configuredCredentials);

                var finalResults = new
                {
                    testRepository = testResults.testRepository,
                    systemCredentials = testResults.systemCredentials,
                    configuredCredentials = testResults.configuredCredentials,
                    overallStatus = overallStatus,
                    recommendation = recommendation,
                    timestamp = testResults.timestamp
                };

                return Ok(new
                {
                    message = "Teste de credenciais concluído",
                    results = finalResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar credenciais");
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Valida um repositório específico e branch
        /// </summary>
        /// <param name="request">Dados do repositório e branch para validação</param>
        /// <returns>Resultado da validação</returns>
        [HttpPost("repository/validate")]
        public async Task<IActionResult> ValidateRepository([FromBody] ValidateRepositoryRequest request)
        {
            try
            {
                _logger.LogInformation("Validando repositório: {RepoUrl}, Branch: {Branch}", 
                    request.RepoUrl, request.Branch);

                if (string.IsNullOrWhiteSpace(request.RepoUrl))
                {
                    return BadRequest(new { message = "RepoUrl é obrigatório" });
                }

                var repoValidation = await _gitHubService.ValidateRepositoryAsync(request.RepoUrl);
                
                object? branchValidation = null;
                if (repoValidation.Success && !string.IsNullOrWhiteSpace(request.Branch))
                {
                    var branchResult = await _gitHubService.ValidateBranchAsync(request.RepoUrl, request.Branch);
                    branchValidation = new
                    {
                        success = branchResult.Success,
                        message = branchResult.Message,
                        branchInfo = branchResult.BranchInfo
                    };
                }

                var validationResult = new
                {
                    repository = new
                    {
                        success = repoValidation.Success,
                        message = repoValidation.Message,
                        repoInfo = repoValidation.RepoInfo
                    },
                    branch = branchValidation,
                    overallValid = repoValidation.Success && (branchValidation == null || ((dynamic)branchValidation).success),
                    timestamp = DateTime.UtcNow
                };

                if (validationResult.overallValid)
                {
                    return Ok(new
                    {
                        message = "Repositório e branch validados com sucesso",
                        validation = validationResult
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Falha na validação do repositório ou branch",
                        validation = validationResult
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar repositório: {RepoUrl}", request.RepoUrl);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Verifica credenciais Git do sistema
        /// </summary>
        private async Task<object> CheckSystemGitCredentialsAsync()
        {
            try
            {
                // Verificar se Git está instalado
                var gitVersion = await RunGitCommandAsync("--version");
                if (!gitVersion.Success)
                {
                    return new
                    {
                        available = false,
                        message = "Git não encontrado no sistema",
                        details = gitVersion.Output
                    };
                }

                // Verificar configurações globais do Git
                var userName = await RunGitCommandAsync("config --global user.name");
                var userEmail = await RunGitCommandAsync("config --global user.email");

                // Verificar credenciais armazenadas (credential helper)
                var credentialHelper = await RunGitCommandAsync("config --global credential.helper");

                return new
                {
                    available = true,
                    gitVersion = gitVersion.Output?.Trim(),
                    globalConfig = new
                    {
                        userName = userName.Success ? userName.Output?.Trim() : null,
                        userEmail = userEmail.Success ? userEmail.Output?.Trim() : null,
                        hasUserConfig = userName.Success && userEmail.Success
                    },
                    credentialHelper = new
                    {
                        configured = credentialHelper.Success && !string.IsNullOrWhiteSpace(credentialHelper.Output),
                        helper = credentialHelper.Success ? credentialHelper.Output?.Trim() : null
                    },
                    message = "Credenciais do sistema verificadas"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar credenciais do sistema");
                return new
                {
                    available = false,
                    message = "Erro ao verificar credenciais do sistema",
                    details = ex.Message
                };
            }
        }

        /// <summary>
        /// Testa credenciais do sistema com um repositório
        /// </summary>
        private async Task<object> TestSystemCredentialsAsync(string testRepoUrl)
        {
            try
            {
                if (!_gitHubSettings.UseSystemCredentials)
                {
                    return new
                    {
                        tested = false,
                        success = false,
                        message = "Credenciais do sistema desabilitadas na configuração",
                        method = "system"
                    };
                }

                var repoResult = await _gitHubService.ValidateRepositoryAsync(testRepoUrl);
                
                return new
                {
                    tested = true,
                    success = repoResult.Success,
                    message = repoResult.Message,
                    details = repoResult.RepoInfo,
                    method = "system"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar credenciais do sistema");
                return new
                {
                    tested = true,
                    success = false,
                    message = "Erro ao testar credenciais do sistema",
                    details = ex.Message,
                    method = "system"
                };
            }
        }

        /// <summary>
        /// Testa credenciais configuradas no appsettings
        /// </summary>
        private async Task<object> TestConfiguredCredentialsAsync(string testRepoUrl)
        {
            try
            {
                if (!_gitHubSettings.HasCredentials)
                {
                    return new
                    {
                        tested = false,
                        success = false,
                        message = "Credenciais não configuradas no appsettings",
                        method = "configured"
                    };
                }

                // Forçar uso de credenciais explícitas para teste
                var repoResult = await _gitHubService.ValidateRepositoryAsync(testRepoUrl);
                
                return new
                {
                    tested = true,
                    success = repoResult.Success,
                    message = repoResult.Message,
                    details = repoResult.RepoInfo,
                    username = _gitHubSettings.Username,
                    method = "configured"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar credenciais configuradas");
                return new
                {
                    tested = true,
                    success = false,
                    message = "Erro ao testar credenciais configuradas",
                    details = ex.Message,
                    username = _gitHubSettings.Username,
                    method = "configured"
                };
            }
        }

        /// <summary>
        /// Determina status geral das credenciais e fornece recomendação
        /// </summary>
        private (string status, string recommendation) DetermineOverallCredentialsStatus(
            dynamic systemTest, dynamic configuredTest)
        {
            var systemWorks = systemTest.success == true;
            var configuredWorks = configuredTest.success == true;
            var configuredTested = configuredTest.tested == true;

            if (systemWorks && configuredWorks)
            {
                return ("excellent", "Ambas as credenciais funcionam. Sistema está configurado corretamente.");
            }
            else if (systemWorks && !configuredWorks && configuredTested)
            {
                return ("good", "Credenciais do sistema funcionam, mas credenciais configuradas têm problemas. Considere corrigir as credenciais no appsettings ou usar apenas credenciais do sistema.");
            }
            else if (systemWorks && !configuredTested)
            {
                return ("good", "Credenciais do sistema funcionam. Para maior controle, considere configurar credenciais específicas no appsettings.");
            }
            else if (!systemWorks && configuredWorks)
            {
                return ("warning", "Apenas credenciais configuradas funcionam. Credenciais do sistema podem precisar de configuração.");
            }
            else if (!systemWorks && !configuredWorks && configuredTested)
            {
                return ("error", "Nenhuma credencial funciona. Verifique as configurações do Git no sistema e as credenciais no appsettings.");
            }
            else
            {
                return ("error", "Não foi possível testar as credenciais adequadamente. Verifique as configurações do sistema.");
            }
        }

        /// <summary>
        /// Executa um comando Git e retorna o resultado
        /// </summary>
        private async Task<(bool Success, string? Output)> RunGitCommandAsync(string arguments)
        {
            try
            {
                using var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    return (true, output);
                }
                else
                {
                    return (false, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comando Git: {Arguments}", arguments);
                return (false, ex.Message);
            }
        }

        #endregion
    }
}
