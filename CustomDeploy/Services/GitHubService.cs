using Microsoft.Extensions.Options;
using CustomDeploy.Models;
using System.Text.Json;
using System.Text;
using System.Diagnostics;

namespace CustomDeploy.Services
{
    /// <summary>
    /// Serviço para integração com GitHub API e validação de repositórios
    /// </summary>
    public class GitHubService
    {
        private readonly GitHubSettings _gitHubSettings;
        private readonly ILogger<GitHubService> _logger;
        private readonly HttpClient _httpClient;

        public GitHubService(IOptions<GitHubSettings> gitHubSettings, ILogger<GitHubService> logger, HttpClient httpClient)
        {
            _gitHubSettings = gitHubSettings.Value;
            _logger = logger;
            _httpClient = httpClient;
            
            // Configurar HttpClient para GitHub API (sem credenciais por padrão)
            _httpClient.BaseAddress = new Uri(_gitHubSettings.ApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CustomDeploy/1.0");
            
            // NÃO configurar credenciais aqui - faremos isso dinamicamente conforme necessário
        }

        /// <summary>
        /// Tenta fazer uma requisição para GitHub com fallback de credenciais
        /// </summary>
        /// <param name="apiPath">Caminho da API (ex: /repos/owner/repo)</param>
        /// <param name="forceExplicitAuth">Força uso de credenciais explícitas</param>
        /// <returns>Resultado da requisição</returns>
        private async Task<(bool Success, string Message, string? Content, bool UsedSystemCredentials)> TryGitHubRequestWithFallbackAsync(string apiPath, bool forceExplicitAuth = false)
        {
            // Se forçando credenciais explícitas, pular tentativa com sistema
            if (!forceExplicitAuth)
            {
                // Estratégia 1: Tentar com credenciais do sistema (sem auth header explícito)
                if (_gitHubSettings.UseSystemCredentials)
                {
                    _logger.LogInformation("🔧 Tentando acesso com credenciais do sistema para: {ApiPath}", apiPath);
                    
                    try
                    {
                        // Remover qualquer header de autorização existente
                        _httpClient.DefaultRequestHeaders.Remove("Authorization");
                        
                        var response = await _httpClient.GetAsync(apiPath);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation("✅ Sucesso com credenciais do sistema: {ApiPath}", apiPath);
                            return (true, "Acesso com credenciais do sistema", content, true);
                        }
                        
                        _logger.LogWarning("❌ Falha com credenciais do sistema: {StatusCode} - {ReasonPhrase}", 
                            response.StatusCode, response.ReasonPhrase);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "❌ Erro com credenciais do sistema: {ApiPath}", apiPath);
                    }
                }
            }

            // Estratégia 2: Tentar com credenciais explícitas do appsettings.json
            if (_gitHubSettings.HasCredentials)
            {
                _logger.LogInformation("Tentando acesso com credenciais explícitas para: {ApiPath}", apiPath);
                
                try
                {
                    // Configurar credenciais explícitas
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_gitHubSettings.Username}:{_gitHubSettings.PersonalAccessToken}"));
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
                    
                    var response = await _httpClient.GetAsync(apiPath);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("✅ Sucesso com credenciais explícitas: {ApiPath}", apiPath);
                        return (true, "Acesso com credenciais explícitas", content, false);
                    }
                    
                    _logger.LogWarning("❌ Falha com credenciais explícitas: {StatusCode} - {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                        
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return (false, "Credenciais explícitas inválidas", null, false);
                    }
                    
                    return (false, $"Erro com credenciais explícitas: {response.StatusCode} - {response.ReasonPhrase}", null, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro com credenciais explícitas: {ApiPath}", apiPath);
                    return (false, $"Erro interno com credenciais explícitas: {ex.Message}", null, false);
                }
            }

            // Ambas as estratégias falharam
            _logger.LogWarning("🚫 Todas as estratégias de autenticação falharam para: {ApiPath}", apiPath);
            return (false, "Nenhuma credencial disponível ou válida", null, false);
        }

        /// <summary>
        /// Valida se um repositório existe e é acessível
        /// </summary>
        /// <param name="repoUrl">URL do repositório (formato: https://github.com/owner/repo.git)</param>
        /// <returns>Resultado da validação</returns>
        public async Task<(bool Success, string Message, object? RepoInfo)> ValidateRepositoryAsync(string repoUrl)
        {
            try
            {
                _logger.LogInformation("Validando repositório: {RepoUrl}", repoUrl);

                var (owner, repo) = ExtractOwnerAndRepoFromUrl(repoUrl);
                
                {if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                    return (false, "URL do repositório inválida. Formato esperado: https://github.com/owner/repo.git", null);
                }

                var apiUrl = $"/repos/{owner}/{repo}";
                var result = await TryGitHubRequestWithFallbackAsync(apiUrl);

                if (result.Success && !string.IsNullOrEmpty(result.Content))
                {
                    var repoInfo = JsonSerializer.Deserialize<JsonElement>(result.Content);
                    
                    var repoData = new
                    {
                        name = repoInfo.GetProperty("name").GetString(),
                        fullName = repoInfo.GetProperty("full_name").GetString(),
                        @private = repoInfo.GetProperty("private").GetBoolean(),
                        defaultBranch = repoInfo.GetProperty("default_branch").GetString(),
                        cloneUrl = repoInfo.GetProperty("clone_url").GetString(),
                        lastPush = repoInfo.GetProperty("pushed_at").GetString(),
                        authMethod = result.UsedSystemCredentials ? "Sistema" : "Explícitas"
                    };

                    _logger.LogInformation("Repositório validado: {FullName}, Privado: {IsPrivate}, Auth: {AuthMethod}", 
                        repoData.fullName, repoData.@private, repoData.authMethod);

                    var message = result.UsedSystemCredentials 
                        ? "Repositório validado com credenciais do sistema"
                        : "Repositório validado com credenciais explícitas";

                    return (true, message, repoData);
                }
                else
                {
                    return (false, result.Message, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar repositório: {RepoUrl}", repoUrl);
                return (false, $"Erro interno ao validar repositório: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Verifica se uma branch existe no repositório
        /// </summary>
        /// <param name="repoUrl">URL do repositório</param>
        /// <param name="branchName">Nome da branch</param>
        /// <returns>Resultado da verificação</returns>
        public async Task<(bool Success, string Message, object? BranchInfo)> ValidateBranchAsync(string repoUrl, string branchName)
        {
            try
            {
                _logger.LogInformation("Validando branch: {Branch} do repositório: {RepoUrl}", branchName, repoUrl);

                var (owner, repo) = ExtractOwnerAndRepoFromUrl(repoUrl);
                if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                {
                    return (false, "URL do repositório inválida", null);
                }

                var apiUrl = $"/repos/{owner}/{repo}/branches/{branchName}";
                var result = await TryGitHubRequestWithFallbackAsync(apiUrl);

                if (result.Success && !string.IsNullOrEmpty(result.Content))
                {
                    var branchInfo = JsonSerializer.Deserialize<JsonElement>(result.Content);
                    
                    var branchData = new
                    {
                        name = branchInfo.GetProperty("name").GetString(),
                        sha = branchInfo.GetProperty("commit").GetProperty("sha").GetString(),
                        protected_ = branchInfo.TryGetProperty("protected", out var protectedProp) ? protectedProp.GetBoolean() : false,
                        authMethod = result.UsedSystemCredentials ? "Sistema" : "Explícitas"
                    };

                    _logger.LogInformation("Branch validada: {Branch}, SHA: {Sha}, Auth: {AuthMethod}", 
                        branchData.name, branchData.sha, branchData.authMethod);

                    var message = result.UsedSystemCredentials 
                        ? "Branch validada com credenciais do sistema"
                        : "Branch validada com credenciais explícitas";

                    return (true, message, branchData);
                }
                else
                {
                    return (false, result.Message, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar branch: {Branch} do repositório: {RepoUrl}", branchName, repoUrl);
                return (false, $"Erro interno ao validar branch: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Gera URL autenticada para clonagem com fallback inteligente
        /// </summary>
        /// <param name="repoUrl">URL original do repositório</param>
        /// <param name="forceExplicitAuth">Força uso de credenciais explícitas</param>
        /// <returns>URL com credenciais embutidas se necessário</returns>
        public string GenerateAuthenticatedCloneUrl(string repoUrl, bool forceExplicitAuth = false)
        {
            // Se forçando credenciais explícitas OU se não deve usar credenciais do sistema
            if (forceExplicitAuth || !_gitHubSettings.UseSystemCredentials)
            {
                if (_gitHubSettings.HasCredentials)
                {
                    try
                    {
                        var uri = new Uri(repoUrl);
                        var authenticatedUrl = $"https://{_gitHubSettings.Username}:{_gitHubSettings.PersonalAccessToken}@{uri.Host}{uri.PathAndQuery}";
                        
                        _logger.LogInformation("🔑 URL autenticada com credenciais explícitas para: {Host}{Path}", uri.Host, uri.PathAndQuery);
                        return authenticatedUrl;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao gerar URL autenticada para: {RepoUrl}", repoUrl);
                        return repoUrl; // Fallback para URL original
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Credenciais explícitas solicitadas mas não configuradas para: {RepoUrl}", repoUrl);
                    return repoUrl;
                }
            }

            // Usar credenciais do sistema (URL original)
            _logger.LogInformation("🔧 Usando credenciais do sistema para: {RepoUrl}", repoUrl);
            return repoUrl;
        }

        /// <summary>
        /// Tenta clone com fallback inteligente de credenciais
        /// </summary>
        /// <param name="repoUrl">URL do repositório</param>
        /// <param name="branch">Branch para clonar</param>
        /// <param name="targetPath">Caminho de destino</param>
        /// <returns>Resultado da operação e método usado</returns>
        public async Task<(bool Success, string Message, bool UsedSystemCredentials)> TryCloneWithFallbackAsync(string repoUrl, string branch, string targetPath)
        {
            _logger.LogInformation("🚀 Iniciando clone com fallback para: {RepoUrl}", repoUrl);

            // Estratégia 1: Tentar com credenciais do sistema
            if (_gitHubSettings.UseSystemCredentials)
            {
                _logger.LogInformation("🔧 Tentando clone com credenciais do sistema...");
                var systemUrl = GenerateAuthenticatedCloneUrl(repoUrl, forceExplicitAuth: false);
                
                var systemResult = await TryCloneDirectAsync(systemUrl, branch, targetPath);
                if (systemResult.Success)
                {
                    _logger.LogInformation("✅ Clone bem-sucedido com credenciais do sistema");
                    return (true, "Clone realizado com credenciais do sistema", true);
                }
                
                _logger.LogWarning("❌ Clone falhou com credenciais do sistema: {Message}", systemResult.Message);
            }

            // Estratégia 2: Tentar com credenciais explícitas
            if (_gitHubSettings.HasCredentials)
            {
                _logger.LogInformation("🔑 Tentando clone com credenciais explícitas...");
                var explicitUrl = GenerateAuthenticatedCloneUrl(repoUrl, forceExplicitAuth: true);
                
                var explicitResult = await TryCloneDirectAsync(explicitUrl, branch, targetPath);
                if (explicitResult.Success)
                {
                    _logger.LogInformation("✅ Clone bem-sucedido com credenciais explícitas");
                    return (true, "Clone realizado com credenciais explícitas", false);
                }
                
                _logger.LogError("❌ Clone falhou com credenciais explícitas: {Message}", explicitResult.Message);
                return (false, $"Falha com credenciais explícitas: {explicitResult.Message}", false);
            }

            _logger.LogError("🚫 Nenhuma estratégia de autenticação disponível");
            return (false, "Nenhuma credencial disponível para clone", false);
        }

        /// <summary>
        /// Executa clone direto sem fallback (método auxiliar)
        /// </summary>
        private async Task<(bool Success, string Message)> TryCloneDirectAsync(string authenticatedUrl, string branch, string targetPath)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone -b {branch} \"{authenticatedUrl}\" \"{targetPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
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
                    var errorMessage = !string.IsNullOrEmpty(error) ? error : output;
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Testa conectividade com GitHub
        /// </summary>
        /// <returns>Resultado do teste</returns>
        /// <summary>
        /// Testa conectividade com GitHub usando sistema de fallback inteligente
        /// </summary>
        /// <returns>Resultado do teste e informações do usuário</returns>
        public async Task<(bool Success, string Message, object? UserInfo)> TestGitHubConnectivityAsync()
        {
            _logger.LogInformation("🔍 Testando conectividade com GitHub usando fallback inteligente");

            // Estratégia 1: Tentar com credenciais do sistema (se habilitado)
            if (_gitHubSettings.UseSystemCredentials)
            {
                _logger.LogInformation("🔧 Testando conectividade com credenciais do sistema...");
                var systemResult = await TryGitHubRequestWithFallbackAsync("/user", forceExplicitAuth: false);
                
                if (systemResult.Success)
                {
                    var userData = ParseUserInfo(systemResult.Content);
                    var login = userData?.GetType().GetProperty("login")?.GetValue(userData)?.ToString() ?? "N/A";
                    _logger.LogInformation("✅ Conectividade OK com credenciais do sistema. Usuário: {Login}", login);
                    return (true, "Conectividade com GitHub estabelecida usando credenciais do sistema", userData);
                }
                
                _logger.LogWarning("❌ Conectividade falhou com credenciais do sistema: {Message}", systemResult.Message);
            }

            // Estratégia 2: Tentar com credenciais explícitas
            if (_gitHubSettings.HasCredentials)
            {
                _logger.LogInformation("🔑 Testando conectividade com credenciais explícitas...");
                var explicitResult = await TryGitHubRequestWithFallbackAsync("/user", forceExplicitAuth: true);
                
                if (explicitResult.Success)
                {
                    var userData = ParseUserInfo(explicitResult.Content);
                    var login = userData?.GetType().GetProperty("login")?.GetValue(userData)?.ToString() ?? "N/A";
                    _logger.LogInformation("✅ Conectividade OK com credenciais explícitas. Usuário: {Login}", login);
                    return (true, "Conectividade com GitHub estabelecida usando credenciais explícitas", userData);
                }
                
                _logger.LogError("❌ Conectividade falhou com credenciais explícitas: {Message}", explicitResult.Message);
                return (false, $"Falha na conectividade com credenciais explícitas: {explicitResult.Message}", null);
            }

            _logger.LogError("🚫 Nenhuma estratégia de autenticação disponível para teste de conectividade");
            return (false, "Nenhuma credencial configurada para teste de conectividade", null);
        }

        /// <summary>
        /// Analisa e extrai informações do usuário da resposta da API
        /// </summary>
        private object? ParseUserInfo(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            try
            {
                var userInfo = JsonSerializer.Deserialize<JsonElement>(content);
                
                return new
                {
                    login = userInfo.GetProperty("login").GetString(),
                    name = userInfo.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "N/A",
                    email = userInfo.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : "N/A",
                    plan = userInfo.TryGetProperty("plan", out var planProp) ? planProp.GetProperty("name").GetString() : "Unknown"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar informações do usuário GitHub");
                return null;
            }
        }

        /// <summary>
        /// Extrai owner e repository name da URL
        /// </summary>
        /// <param name="repoUrl">URL do repositório</param>
        /// <returns>Tupla com (owner, repository)</returns>
        private (string owner, string repo) ExtractOwnerAndRepoFromUrl(string repoUrl)
        {
            try
            {
                var uri = new Uri(repoUrl);
                var pathParts = uri.AbsolutePath.Trim('/').Split('/');
                
                if (pathParts.Length >= 2)
                {
                    var owner = pathParts[0];
                    var repo = pathParts[1].Replace(".git", ""); // Remove .git se presente
                    return (owner, repo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao extrair owner/repo da URL: {RepoUrl}", repoUrl);
            }

            return (string.Empty, string.Empty);
        }
    }
}
