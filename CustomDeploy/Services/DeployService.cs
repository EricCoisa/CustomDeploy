using System.Diagnostics;
using System.Text;
using CustomDeploy.Models;

namespace CustomDeploy.Services
{
    public class DeployService
    {
        private readonly ILogger<DeployService> _logger;
        private readonly IISManagementService _iisManagementService;
        private readonly GitHubService _gitHubService;
        private readonly string _workingDirectory;
        private readonly string _publicationsPath;

        public DeployService(ILogger<DeployService> logger, IISManagementService iisManagementService, GitHubService gitHubService, IConfiguration configuration)
        {
            _logger = logger;
            _iisManagementService = iisManagementService;
            _gitHubService = gitHubService;
            _workingDirectory = configuration.GetValue<string>("DeploySettings:WorkingDirectory") 
                ?? Path.Combine(Path.GetTempPath(), "CustomDeploy");
            _publicationsPath = configuration.GetValue<string>("DeploySettings:PublicationsPath") 
                ?? "C:\\temp\\wwwroot";
            
            // Criar diretório de trabalho se não existir
            if (!Directory.Exists(_workingDirectory))
            {
                Directory.CreateDirectory(_workingDirectory);
            }
            
            _logger.LogInformation("DeployService inicializado.");
        }

        public async Task<(bool Success, string Message, object? DeployDetails)> ExecuteDeployAsync(DeployRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var repoName = GetRepoNameFromUrl(request.RepoUrl);
            var repoPath = Path.Combine(_workingDirectory, repoName);
            var deployDetails = new
            {
                siteInfo = (object?)null,
                applicationInfo = (object?)null,
                finalPath = string.Empty,
                isIISApplication = false,
                warnings = new List<string>(),
                parsedSite = string.Empty,
                parsedApplication = string.Empty
            };

            try
            {
                _logger.LogInformation("Iniciando deploy para repositório: {RepoUrl} no site IIS: {IisSiteName}", 
                    request.RepoUrl, request.IisSiteName);

                // 1. Parsear site e aplicação
                var (siteName, applicationPath) = ParseSiteAndApplication(request.IisSiteName, request.ApplicationPath);
                _logger.LogInformation("Site parseado: {SiteName}, Aplicação: {ApplicationPath}", 
                    siteName, applicationPath ?? "nenhuma");

                deployDetails = deployDetails with 
                { 
                    parsedSite = siteName,
                    parsedApplication = applicationPath ?? string.Empty
                };

                // 2. Verificar se o site IIS existe e obter informações
                var siteResult = await _iisManagementService.GetSiteInfoAsync(siteName);
                if (!siteResult.Success)
                {
                    return (false, $"Site não encontrado no IIS: {siteName}. {siteResult.Message}", deployDetails);
                }

                var sitePhysicalPath = siteResult.PhysicalPath;
                _logger.LogInformation("Site IIS encontrado: {SiteName}, Caminho físico: {PhysicalPath}", 
                    siteName, sitePhysicalPath);

                // 3. Determinar caminho final baseado na aplicação parseada
                string finalTargetPath;
                if (!string.IsNullOrWhiteSpace(applicationPath))
                {
                    // Se há uma aplicação parseada (ex: carteira de gruppy/carteira), usar ela como base
                    finalTargetPath = Path.Combine(sitePhysicalPath, applicationPath);
                    
                    // Só adicionar targetPath se for diferente da applicationPath (evitar duplicação)
                    if (!string.IsNullOrWhiteSpace(request.TargetPath) && 
                        !string.Equals(request.TargetPath, applicationPath, StringComparison.OrdinalIgnoreCase))
                    {
                        finalTargetPath = Path.Combine(finalTargetPath, request.TargetPath);
                        _logger.LogInformation("Combinando caminhos: {ApplicationPath} + {TargetPath}", 
                            applicationPath, request.TargetPath);
                    }
                    else if (string.Equals(request.TargetPath, applicationPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("TargetPath igual ao ApplicationPath, usando apenas: {Path}", 
                            applicationPath);
                    }
                    else
                    {
                        _logger.LogInformation("Usando apenas ApplicationPath: {Path}", applicationPath);
                    }
                }
                else
                {
                    // Comportamento original: site + targetPath
                    finalTargetPath = Path.Combine(sitePhysicalPath, request.TargetPath ?? string.Empty);
                }

                deployDetails = deployDetails with 
                { 
                    siteInfo = siteResult.SiteInfo,
                    finalPath = finalTargetPath
                };

                // 4. Verificar se a aplicação parseada existe no IIS
                if (!string.IsNullOrWhiteSpace(applicationPath))
                {
                    var appResult = await _iisManagementService.CheckApplicationExistsAsync(siteName, applicationPath);
                    if (appResult.Success)
                    {
                        if (appResult.ApplicationExists)
                        {
                            _logger.LogInformation("Aplicação IIS encontrada: {SiteName}/{ApplicationPath}", 
                                siteName, applicationPath);
                            deployDetails = deployDetails with 
                            { 
                                applicationInfo = appResult.ApplicationInfo,
                                isIISApplication = true
                            };
                        }
                        else
                        {
                            var warning = $"Aplicação '{applicationPath}' não existe como subaplicação no IIS, será tratada como pasta.";
                            _logger.LogWarning(warning);
                            var warnings = deployDetails.warnings.ToList();
                            warnings.Add(warning);
                            deployDetails = deployDetails with { warnings = warnings };
                        }
                    }
                    else
                    {
                        var warning = $"Erro ao verificar aplicação IIS: {appResult.Message}";
                        _logger.LogWarning(warning);
                        var warnings = deployDetails.warnings.ToList();
                        warnings.Add(warning);
                        deployDetails = deployDetails with { warnings = warnings };
                    }
                }

                // 5. Verificar targetPath adicional como aplicação IIS (se diferente da aplicação principal)
                if (!string.IsNullOrWhiteSpace(request.TargetPath) && request.TargetPath != applicationPath)
                {
                    var targetCheckPath = !string.IsNullOrWhiteSpace(applicationPath) 
                        ? $"{applicationPath}/{request.TargetPath}"
                        : request.TargetPath;
                        
                    var appResult = await _iisManagementService.CheckApplicationExistsAsync(siteName, targetCheckPath);
                    if (appResult.Success && appResult.ApplicationExists)
                    {
                        _logger.LogInformation("Aplicação IIS encontrada no targetPath: {SiteName}/{TargetPath}", 
                            siteName, targetCheckPath);
                        // Atualizar apenas se não já identificamos uma aplicação
                        if (!deployDetails.isIISApplication)
                        {
                            deployDetails = deployDetails with 
                            { 
                                applicationInfo = appResult.ApplicationInfo,
                                isIISApplication = true
                            };
                        }
                    }
                }

                // 6. Clonar ou atualizar repositório
                var gitResult = await CloneOrUpdateRepositoryAsync(request.RepoUrl, request.Branch, repoPath);
                if (!gitResult.Success)
                {
                    return (false, $"Erro no Git: {gitResult.Message}", deployDetails);
                }

                // 7. Executar comandos de build
                var buildResult = await ExecuteBuildCommandsAsync(request.BuildCommands, repoPath);
                if (!buildResult.Success)
                {
                    return (false, $"Erro no build: {buildResult.Message}", deployDetails);
                }

                // 8. Copiar arquivos para destino (usando o caminho do IIS)
                var copyResult = await CopyBuildOutputToIISPathAsync(repoPath, request.BuildOutput, finalTargetPath);
                if (!copyResult.Success)
                {
                    return (false, $"Erro na cópia: {copyResult.Message}", deployDetails);
                }

                stopwatch.Stop();
                var successMessage = $"Deploy concluído com sucesso em {stopwatch.Elapsed.TotalSeconds:F2} segundos";
                _logger.LogInformation(successMessage);
                
                return (true, successMessage, deployDetails);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erro durante o deploy");
                return (false, $"Erro interno: {ex.Message}", deployDetails);
            }
        }

        private async Task<(bool Success, string Message)> CloneOrUpdateRepositoryAsync(string repoUrl, string branch, string repoPath)
        {
            try
            {
                // Validar repositório primeiro
                var validationResult = await _gitHubService.ValidateRepositoryAsync(repoUrl);
                if (!validationResult.Success)
                {
                    _logger.LogWarning("Validação do repositório falhou: {Message}", validationResult.Message);
                    // Continuar mesmo assim para compatibilidade com repos não-GitHub ou credenciais de sistema
                }
                else
                {
                    _logger.LogInformation("Repositório validado: {Message}", validationResult.Message);
                }

                // Gerar URL autenticada se necessário
                var authenticatedUrl = _gitHubService.GenerateAuthenticatedCloneUrl(repoUrl);

                if (Directory.Exists(repoPath))
                {
                    _logger.LogInformation("Repositório já existe, atualizando: {RepoPath}", repoPath);
                    
                    // Checkout da branch e pull
                    var checkoutResult = await RunGitCommandAsync($"checkout {branch}", repoPath);
                    if (!checkoutResult.Success)
                    {
                        return (false, $"Falha no checkout: {checkoutResult.Message}");
                    }

                    var pullResult = await RunGitCommandAsync("pull", repoPath);
                    if (!pullResult.Success)
                    {
                        return (false, $"Falha no pull: {pullResult.Message}");
                    }

                    return (true, "Repositório atualizado com sucesso");
                }
                else
                {
                    _logger.LogInformation("🚀 Clonando repositório com fallback inteligente: {RepoUrl}", repoUrl);
                    
                    // Usar o novo método de clone com fallback
                    var cloneResult = await _gitHubService.TryCloneWithFallbackAsync(repoUrl, branch, repoPath);
                    if (!cloneResult.Success)
                    {
                        return (false, $"Falha no clone: {cloneResult.Message}");
                    }

                    var authMethod = cloneResult.UsedSystemCredentials ? "credenciais do sistema" : "credenciais explícitas";
                    _logger.LogInformation("✅ Clone realizado com sucesso usando {AuthMethod}", authMethod);
                    return (true, $"Repositório clonado com sucesso usando {authMethod}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante operação Git");
                return (false, ex.Message);
            }
        }

        private async Task<(bool Success, string Message)> RunGitCommandAsync(string arguments, string workingDirectory)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    outputBuilder.AppendLine(args.Data);
                    _logger.LogDebug("Git Output: {Output}", args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    errorBuilder.AppendLine(args.Data);
                    _logger.LogDebug("Git Error: {Error}", args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

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

        private async Task<(bool Success, string Message)> ExecuteBuildCommandsAsync(string[] buildCommands, string workingDirectory)
        {
            try
            {
                if (buildCommands == null || buildCommands.Length == 0)
                {
                    return (true, "Nenhum comando de build especificado");
                }

                var allOutput = new StringBuilder();
                
                for (int i = 0; i < buildCommands.Length; i++)
                {
                    var buildCommand = buildCommands[i];
                    _logger.LogInformation("Executando comando de build {Index}/{Total}: {BuildCommand}", 
                        i + 1, buildCommands.Length, buildCommand);

                    var result = await ExecuteBuildCommandAsync(buildCommand, workingDirectory);
                    
                    allOutput.AppendLine($"Comando {i + 1}: {buildCommand}");
                    allOutput.AppendLine($"Resultado: {(result.Success ? "Sucesso" : "Falha")}");
                    allOutput.AppendLine($"Mensagem: {result.Message}");
                    allOutput.AppendLine("---");

                    if (!result.Success)
                    {
                        return (false, $"Falha no comando {i + 1} ({buildCommand}): {result.Message}");
                    }
                }

                return (true, $"Todos os {buildCommands.Length} comandos executados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comandos de build");
                return (false, $"Erro inesperado: {ex.Message}");
            }
        }

        private async Task<(bool Success, string Message)> ExecuteBuildCommandAsync(string buildCommand, string workingDirectory)
        {
            try
            {
                _logger.LogInformation("Executando comando de build: {BuildCommand}", buildCommand);

                // Detectar se é um comando npm/yarn/dotnet e configurar adequadamente
                var (fileName, arguments) = PrepareCommand(buildCommand);

                var processInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Adicionar caminhos comuns ao PATH se necessário
                if (buildCommand.StartsWith("npm") || buildCommand.StartsWith("yarn"))
                {
                    var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                    var nodePaths = ";C:\\Program Files\\nodejs;C:\\Program Files (x86)\\nodejs;" +
                                   $"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\npm";
                    processInfo.Environment["PATH"] = currentPath + nodePaths;
                }

                using var process = new Process { StartInfo = processInfo };
                
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        outputBuilder.AppendLine(args.Data);
                        _logger.LogInformation("Build Output: {Output}", args.Data);
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        errorBuilder.AppendLine(args.Data);
                        _logger.LogWarning("Build Error: {Error}", args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                var output = outputBuilder.ToString();
                var error = errorBuilder.ToString();

                _logger.LogInformation("Processo finalizado com código: {ExitCode}", process.ExitCode);

                if (process.ExitCode == 0)
                {
                    return (true, $"Build executado com sucesso.\n{output}");
                }
                else
                {
                    var errorMessage = !string.IsNullOrEmpty(error) ? error : output;
                    return (false, $"Build falhou (código: {process.ExitCode}):\n{errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante execução do build");
                return (false, ex.Message);
            }
        }

        private (string fileName, string arguments) PrepareCommand(string command)
        {
            // Detectar comandos "start cmd" e remover o "start" para aguardar execução completa
            if (command.StartsWith("start cmd "))
            {
                _logger.LogInformation("Detectado comando 'start cmd', removendo 'start' para aguardar execução completa");
                
                // Remover "start " do início, mantendo "cmd ..."
                command = command.Substring(6); // Remove "start "
                
                // Adicionar /c se não existir
                if (!command.Contains("cmd /c"))
                {
                    if (command.StartsWith("cmd \""))
                    {
                        // Padrão: cmd "comando" -> cmd /c "comando"
                        command = command.Replace("cmd \"", "cmd /c \"");
                    }
                    else
                    {
                        // Padrão: cmd comando -> cmd /c comando
                        command = command.Replace("cmd ", "cmd /c ");
                    }
                    _logger.LogInformation("Adicionado /c ao comando cmd");
                }
                
                _logger.LogInformation("Comando modificado para aguardar execução: {ModifiedCommand}", command);
            }
            
            // Para comandos que precisam do shell (npm, yarn, etc.)
            if (command.StartsWith("npm ") || 
                command.StartsWith("yarn ") || 
                command.StartsWith("npx ") ||
                command.StartsWith("cmd ") ||
                command.Contains("&&") ||
                command.Contains("&") ||
                command.Contains("|"))
            {
                return ("cmd.exe", $"/c \"{command}\"");
            }
            
            // Para comandos diretos (dotnet, git, etc.)
            var parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
                return ("cmd.exe", "/c echo No command specified");
            
            if (parts.Length == 1)
                return (parts[0], "");
            
            return (parts[0], parts[1]);
        }

        private async Task<(bool Success, string Message)> CopyBuildOutputToIISPathAsync(string repoPath, string buildOutput, string targetPath)
        {
            try
            {
                var sourcePath = Path.Combine(repoPath, buildOutput);
                
                if (!Directory.Exists(sourcePath))
                {
                    return (false, $"Diretório de build não encontrado: {sourcePath}");
                }

                _logger.LogInformation("Copiando arquivos de {SourcePath} para caminho IIS: {TargetPath}", sourcePath, targetPath);

                // Validar se o diretório pai existe (site físico)
                var parentPath = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(parentPath))
                {
                    return (false, $"Diretório do site IIS não encontrado: {parentPath}");
                }

                // Limpar diretório de destino se existir
                if (Directory.Exists(targetPath))
                {
                    _logger.LogInformation("Limpando diretório de destino IIS: {TargetPath}", targetPath);
                    Directory.Delete(targetPath, true);
                    await Task.Delay(100); // Pequeno delay para garantir que o diretório foi deletado
                }

                // Criar diretório de destino
                Directory.CreateDirectory(targetPath);

                // Copiar recursivamente
                await CopyDirectoryAsync(sourcePath, targetPath);

                _logger.LogInformation("Deploy para IIS concluído: {TargetPath}", targetPath);
                return (true, $"Arquivos copiados com sucesso para {targetPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante cópia dos arquivos para IIS");
                return (false, ex.Message);
            }
        }

        private async Task CopyDirectoryAsync(string sourceDir, string targetDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            var dirs = dir.GetDirectories();

            // Criar diretório de destino
            Directory.CreateDirectory(targetDir);

            // Copiar arquivos
            foreach (var file in dir.GetFiles())
            {
                var targetFilePath = Path.Combine(targetDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // Copiar subdiretórios recursivamente
            foreach (var subDir in dirs)
            {
                var targetSubDir = Path.Combine(targetDir, subDir.Name);
                await CopyDirectoryAsync(subDir.FullName, targetSubDir);
            }
        }

        private string GetRepoNameFromUrl(string repoUrl)
        {
            // Extrair nome do repositório da URL
            // Ex: https://github.com/user/repo.git -> repo
            var uri = new Uri(repoUrl);
            var segments = uri.Segments;
            var lastSegment = segments[segments.Length - 1];
            
            // Remover .git se existir
            if (lastSegment.EndsWith(".git"))
                lastSegment = lastSegment.Substring(0, lastSegment.Length - 4);
            
            return lastSegment;
        }

        /// <summary>
        /// Parseia o nome do site e aplicação baseado na sintaxe "site/aplicacao" ou campos separados
        /// </summary>
        /// <param name="iisSiteName">Nome do site IIS que pode conter "/" para separar site/aplicação</param>
        /// <param name="applicationPath">Caminho específico da aplicação (opcional)</param>
        /// <returns>Tupla com (nome_do_site, caminho_da_aplicacao)</returns>
        private (string siteName, string? applicationPath) ParseSiteAndApplication(string iisSiteName, string? applicationPath)
        {
            // Se applicationPath foi especificado explicitamente, usar ele
            if (!string.IsNullOrWhiteSpace(applicationPath))
            {
                return (iisSiteName, applicationPath);
            }

            // Se não, verificar se iisSiteName contém "/"
            if (iisSiteName.Contains('/'))
            {
                var parts = iisSiteName.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    return (parts[0], parts[1]);
                }
            }

            // Caso padrão: apenas site, sem aplicação
            return (iisSiteName, null);
        }
    }
}
