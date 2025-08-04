using CustomDeploy.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CustomDeploy.Services
{
    public class DeployService
    {
        private readonly ILogger<DeployService> _logger;
        private readonly IISManagementService _iisManagementService;
        private readonly GitHubService _gitHubService;
        private readonly string _workingDirectory;
        private readonly string _publicationsPath;
        private readonly string _deploysJsonPath;
        private static readonly object _deploysFileLock = new object();

        public DeployService(ILogger<DeployService> logger, IISManagementService iisManagementService, GitHubService gitHubService, IConfiguration configuration)
        {
            _logger = logger;
            _iisManagementService = iisManagementService;
            _gitHubService = gitHubService;
            _workingDirectory = configuration.GetValue<string>("DeploySettings:WorkingDirectory") 
                ?? Path.Combine(Path.GetTempPath(), "CustomDeploy");
            _publicationsPath = configuration.GetValue<string>("DeploySettings:PublicationsPath") 
                ?? "C:\\temp\\wwwroot";
            
            // Caminho para o arquivo centralizado de metadados
            _deploysJsonPath = Path.Combine(AppContext.BaseDirectory, "deploys.json");
            
            // Criar diretório de trabalho se não existir
            if (!Directory.Exists(_workingDirectory))
            {
                Directory.CreateDirectory(_workingDirectory);
            }
            
            // Garantir que o diretório do arquivo de metadados existe
            var deploysDirectory = Path.GetDirectoryName(_deploysJsonPath);
            if (!string.IsNullOrEmpty(deploysDirectory) && !Directory.Exists(deploysDirectory))
            {
                Directory.CreateDirectory(deploysDirectory);
                _logger.LogInformation("Diretório de metadados criado: {Directory}", deploysDirectory);
            }
            
            _logger.LogInformation("DeployService inicializado. Arquivo de metadados: {DeploysJsonPath}", _deploysJsonPath);
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

                // 7. Executar comando de build
                var buildResult = await ExecuteBuildCommandAsync(request.BuildCommand, repoPath);
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

                // 9. Salvar metadados do deploy
                var metadataResult = SaveDeployMetadata(request, finalTargetPath);
                if (!metadataResult.Success)
                {
                    _logger.LogWarning("Falha ao salvar metadados: {Message}", metadataResult.Message);
                    // Não falha o deploy por causa dos metadados
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

        private async Task<(bool Success, string Message)> CopyBuildOutputAsync(string repoPath, string buildOutput, string targetPath)
        {
            try
            {
                var sourcePath = Path.Combine(repoPath, buildOutput);
                
                if (!Directory.Exists(sourcePath))
                {
                    return (false, $"Diretório de build não encontrado: {sourcePath}");
                }

                _logger.LogInformation("Copiando arquivos de {SourcePath} para {TargetPath}", sourcePath, targetPath);

                // Limpar diretório de destino se existir
                if (Directory.Exists(targetPath))
                {
                    _logger.LogInformation("Limpando diretório de destino: {TargetPath}", targetPath);
                    Directory.Delete(targetPath, true);
                    await Task.Delay(100); // Pequeno delay para garantir que o diretório foi deletado
                }

                // Criar diretório de destino
                Directory.CreateDirectory(targetPath);

                // Copiar recursivamente
                await CopyDirectoryAsync(sourcePath, targetPath);

                return (true, "Arquivos copiados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante cópia dos arquivos");
                return (false, ex.Message);
            }
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

        private (string fileName, string arguments) SplitCommand(string command)
        {
            var parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
                return ("", "");
            
            if (parts.Length == 1)
                return (parts[0], "");
            
            return (parts[0], parts[1]);
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

        private (bool Success, string Message) SaveDeployMetadata(DeployRequest request)
        {
            try
            {
                var normalizedTargetPath = Path.GetFullPath(request.TargetPath);
                
                // Calcular o nome do deploy baseado no caminho relativo
                var relativePath = GetRelativePathFromTargetPath(request.TargetPath);
                var deployName = string.IsNullOrWhiteSpace(relativePath) ? Path.GetFileName(normalizedTargetPath) : relativePath;
                
                var metadata = new DeployMetadata
                {
                    Name = deployName,
                    Repository = request.RepoUrl,
                    Branch = request.Branch,
                    BuildCommand = request.BuildCommand,
                    BuildOutput = request.BuildOutput,
                    TargetPath = normalizedTargetPath,
                    DeployedAt = DateTime.UtcNow
                };

                // Thread-safe operation para ler/escrever o arquivo de metadados
                lock (_deploysFileLock)
                {
                    var deploysList = LoadAllDeployMetadata();
                    
                    // Remover entrada existente se houver (baseado no targetPath)
                    deploysList.RemoveAll(d => string.Equals(d.TargetPath, normalizedTargetPath, StringComparison.OrdinalIgnoreCase));
                    
                    // Adicionar nova entrada
                    deploysList.Add(metadata);
                    
                    // Salvar de volta
                    SaveAllDeployMetadata(deploysList);
                }

                _logger.LogInformation("Metadados do deploy salvos no arquivo centralizado: {DeploysJsonPath}", _deploysJsonPath);
                return (true, "Metadados salvos com sucesso no arquivo centralizado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar metadados do deploy no arquivo centralizado");
                return (false, ex.Message);
            }
        }

        private (bool Success, string Message) SaveDeployMetadata(DeployRequest request, string finalTargetPath)
        {
            try
            {
                var normalizedTargetPath = Path.GetFullPath(finalTargetPath);
                
                // Para deploys IIS, usar o nome do site + targetPath como nome do deploy
                var deployName = string.IsNullOrWhiteSpace(request.TargetPath) 
                    ? request.IisSiteName 
                    : $"{request.IisSiteName}/{request.TargetPath.Trim('/', '\\')}";
                
                // Remover barras extras no final
                deployName = deployName.TrimEnd('/', '\\');
                
                var metadata = new DeployMetadata
                {
                    Name = deployName,
                    Repository = request.RepoUrl,
                    Branch = request.Branch,
                    BuildCommand = request.BuildCommand,
                    BuildOutput = request.BuildOutput,
                    TargetPath = normalizedTargetPath,
                    DeployedAt = DateTime.UtcNow
                };

                // Thread-safe operation para ler/escrever o arquivo de metadados
                lock (_deploysFileLock)
                {
                    var deploysList = LoadAllDeployMetadata();
                    
                    // Remover entrada existente se houver (baseado no targetPath)
                    deploysList.RemoveAll(d => string.Equals(d.TargetPath, normalizedTargetPath, StringComparison.OrdinalIgnoreCase));
                    
                    // Adicionar nova entrada
                    deploysList.Add(metadata);
                    
                    // Salvar de volta
                    SaveAllDeployMetadata(deploysList);
                }

                _logger.LogInformation("Metadados do deploy IIS salvos: {DeployName} -> {TargetPath}", deployName, normalizedTargetPath);
                return (true, "Metadados do deploy IIS salvos com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar metadados do deploy IIS");
                return (false, ex.Message);
            }
        }

        private List<DeployMetadata> LoadAllDeployMetadata()
        {
            try
            {
                if (!File.Exists(_deploysJsonPath))
                {
                    _logger.LogInformation("Arquivo de metadados não existe, criando novo: {DeploysJsonPath}", _deploysJsonPath);
                    
                    // Criar arquivo vazio imediatamente
                    var emptyList = new List<DeployMetadata>();
                    SaveAllDeployMetadata(emptyList);
                    
                    return emptyList;
                }

                var jsonContent = File.ReadAllText(_deploysJsonPath, Encoding.UTF8);
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    // Se arquivo existe mas está vazio, criar conteúdo inicial
                    var emptyList = new List<DeployMetadata>();
                    SaveAllDeployMetadata(emptyList);
                    return emptyList;
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var deploysList = JsonSerializer.Deserialize<List<DeployMetadata>>(jsonContent, jsonOptions);
                return deploysList ?? new List<DeployMetadata>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar metadados do arquivo centralizado: {DeploysJsonPath}", _deploysJsonPath);
                return new List<DeployMetadata>();
            }
        }

        private void SaveAllDeployMetadata(List<DeployMetadata> deploysList)
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonContent = JsonSerializer.Serialize(deploysList, jsonOptions);
                
                // Garantir que o diretório existe antes de salvar
                var directory = Path.GetDirectoryName(_deploysJsonPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("Diretório criado para arquivo de metadados: {Directory}", directory);
                }
                
                File.WriteAllText(_deploysJsonPath, jsonContent, Encoding.UTF8);
                
                _logger.LogInformation("Arquivo de metadados salvo com sucesso: {Path} ({Count} entradas)", 
                    _deploysJsonPath, deploysList.Count);
                _logger.LogDebug("Conteúdo salvo: {JsonContent}", jsonContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar arquivo de metadados: {Path}", _deploysJsonPath);
                throw;
            }
        }

        public List<DeployMetadata> GetAllDeployMetadata()
        {
            lock (_deploysFileLock)
            {
                return LoadAllDeployMetadata();
            }
        }

        /// <summary>
        /// Testa o sistema de metadados e força a criação do arquivo deploys.json se necessário
        /// </summary>
        /// <returns>Informações sobre o teste</returns>
        public (bool Success, string Message, string FilePath) TestMetadataSystem()
        {
            try
            {
                _logger.LogInformation("Testando sistema de metadados...");
                
                lock (_deploysFileLock)
                {
                    // Forçar carregamento/criação do arquivo
                    var deploysList = LoadAllDeployMetadata();
                    
                    // Verificar se arquivo foi criado
                    var fileExists = File.Exists(_deploysJsonPath);
                    var fileInfo = fileExists ? new FileInfo(_deploysJsonPath) : null;
                    
                    var message = $"Sistema de metadados testado. " +
                                 $"Arquivo: {(fileExists ? "Existe" : "Não existe")}, " +
                                 $"Caminho: {_deploysJsonPath}, " +
                                 $"Entradas carregadas: {deploysList.Count}";
                    
                    if (fileExists && fileInfo != null)
                    {
                        message += $", Tamanho: {fileInfo.Length} bytes, Modificado: {fileInfo.LastWriteTime}";
                    }
                    
                    _logger.LogInformation(message);
                    return (true, message, _deploysJsonPath);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Erro ao testar sistema de metadados: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage, _deploysJsonPath);
            }
        }

        public List<DeployMetadata> GetAllDeployMetadataWithExistsCheck()
        {
            lock (_deploysFileLock)
            {
                var deploysList = LoadAllDeployMetadata();
                bool hasChanges = false;

                // Verificar e atualizar o campo exists para cada deploy
                foreach (var deploy in deploysList)
                {
                    var normalizedPath = Path.GetFullPath(deploy.TargetPath);
                    var currentExists = Directory.Exists(normalizedPath);
                    
                    if (deploy.Exists != currentExists)
                    {
                        deploy.Exists = currentExists;
                        hasChanges = true;
                        _logger.LogInformation("Status de existência atualizado para {Name}: {Exists}", 
                            deploy.Name, currentExists);
                    }
                }

                // Persistir mudanças se houver
                if (hasChanges)
                {
                    SaveAllDeployMetadata(deploysList);
                    _logger.LogInformation("Arquivo de metadados atualizado com status de existência");
                }

                return deploysList;
            }
        }

        public (bool Success, string Message) RemoveDeployMetadata(string name)
        {
            try
            {
                lock (_deploysFileLock)
                {
                    var deploysList = LoadAllDeployMetadata();
                    var removedCount = deploysList.RemoveAll(d => 
                        string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

                    if (removedCount == 0)
                    {
                        return (false, $"Deploy com nome '{name}' não encontrado");
                    }

                    SaveAllDeployMetadata(deploysList);
                    _logger.LogInformation("Deploy removido dos metadados: {Name}", name);
                    
                    return (true, $"Deploy '{name}' removido com sucesso dos metadados");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover deploy dos metadados: {Name}", name);
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message, DeployMetadata? Deploy) GetDeployMetadata(string name)
        {
            try
            {
                lock (_deploysFileLock)
                {
                    var deploysList = LoadAllDeployMetadata();
                    var deploy = deploysList.FirstOrDefault(d => 
                        string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

                    if (deploy == null)
                    {
                        return (false, $"Deploy com nome '{name}' não encontrado", null);
                    }

                    // Verificar existência atual
                    var normalizedPath = Path.GetFullPath(deploy.TargetPath);
                    deploy.Exists = Directory.Exists(normalizedPath);

                    return (true, "Deploy encontrado", deploy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar deploy: {Name}", name);
                return (false, ex.Message, null);
            }
        }

        public (bool Success, string Message) CreateMetadataForExistingDirectory(string directoryPath)
        {
            try
            {
                var normalizedPath = Path.GetFullPath(directoryPath);
                var directoryInfo = new DirectoryInfo(normalizedPath);
                
                if (!directoryInfo.Exists)
                {
                    return (false, $"Diretório não existe: {normalizedPath}");
                }

                lock (_deploysFileLock)
                {
                    var deploysList = LoadAllDeployMetadata();
                    
                    // Verificar se já existe entrada para este caminho
                    var existingDeploy = deploysList.FirstOrDefault(d => 
                        string.Equals(d.TargetPath, normalizedPath, StringComparison.OrdinalIgnoreCase));
                    
                    if (existingDeploy != null)
                    {
                        return (true, "Metadados já existem para este diretório");
                    }

                    // Criar nova entrada de metadados
                    var relativePath = GetRelativePathFromFullPath(normalizedPath);
                    var deployName = string.IsNullOrWhiteSpace(relativePath) ? directoryInfo.Name : relativePath;
                    
                    var metadata = new DeployMetadata
                    {
                        Name = deployName,
                        Repository = "N/A (Criado automaticamente)",
                        Branch = "N/A",
                        BuildCommand = "N/A",
                        BuildOutput = "N/A",
                        TargetPath = normalizedPath,
                        DeployedAt = directoryInfo.CreationTime,
                        Exists = true
                    };

                    deploysList.Add(metadata);
                    SaveAllDeployMetadata(deploysList);
                    
                    _logger.LogInformation("Metadados criados automaticamente para diretório existente: {Path}", normalizedPath);
                    return (true, $"Metadados criados automaticamente para '{directoryInfo.Name}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar metadados automáticos para diretório: {Path}", directoryPath);
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) DeletePublicationCompletely(string name)
        {
            try
            {
                lock (_deploysFileLock)
                {
                    var deploysList = LoadAllDeployMetadata();
                    var deployToDelete = deploysList.FirstOrDefault(d => 
                        string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

                    if (deployToDelete == null)
                    {
                        return (false, $"Deploy com nome '{name}' não encontrado nos metadados");
                    }

                    var targetPath = deployToDelete.TargetPath;
                    var physicalDeletionSuccess = false;
                    var physicalDeletionMessage = "";

                    // Tentar deletar a pasta física se existir
                    if (Directory.Exists(targetPath))
                    {
                        try
                        {
                            _logger.LogInformation("Deletando pasta física: {TargetPath}", targetPath);
                            Directory.Delete(targetPath, true);
                            physicalDeletionSuccess = true;
                            physicalDeletionMessage = "Pasta física deletada com sucesso";
                            _logger.LogInformation("Pasta física deletada: {TargetPath}", targetPath);
                        }
                        catch (Exception ex)
                        {
                            physicalDeletionMessage = $"Erro ao deletar pasta física: {ex.Message}";
                            _logger.LogError(ex, "Erro ao deletar pasta física: {TargetPath}", targetPath);
                        }
                    }
                    else
                    {
                        physicalDeletionSuccess = true;
                        physicalDeletionMessage = "Pasta física não existe (já removida ou não encontrada)";
                        _logger.LogInformation("Pasta física não encontrada: {TargetPath}", targetPath);
                    }

                    // Remover dos metadados independentemente do sucesso da exclusão física
                    var removedCount = deploysList.RemoveAll(d => 
                        string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

                    if (removedCount > 0)
                    {
                        SaveAllDeployMetadata(deploysList);
                        _logger.LogInformation("Deploy removido dos metadados: {Name}", name);
                        
                        if (physicalDeletionSuccess)
                        {
                            return (true, $"Deploy '{name}' removido completamente (metadados e pasta física)");
                        }
                        else
                        {
                            return (true, $"Deploy '{name}' removido dos metadados. {physicalDeletionMessage}");
                        }
                    }
                    else
                    {
                        return (false, $"Falha ao remover deploy '{name}' dos metadados");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar publicação completamente: {Name}", name);
                return (false, ex.Message);
            }
        }

        private string? ValidateAndResolveTargetPath(string targetPath)
        {
            try
            {
                // Remover barras iniciais e finais
                targetPath = targetPath.Trim('\\', '/');

                // Verificar se não está tentando usar caminhos relativos perigosos
                if (targetPath.Contains("..") || 
                    targetPath.Contains(":") || 
                    targetPath.StartsWith("\\") || 
                    targetPath.StartsWith("/") ||
                    Path.IsPathFullyQualified(targetPath))
                {
                    _logger.LogWarning("TargetPath inválido detectado: {TargetPath}", targetPath);
                    return null;
                }

                // Combinar com o publications path
                var resolvedPath = Path.Combine(_publicationsPath, targetPath);
                var normalizedPath = Path.GetFullPath(resolvedPath);

                // Verificar se o caminho resolvido ainda está dentro do publications path
                var normalizedPublicationsPath = Path.GetFullPath(_publicationsPath);
                if (!normalizedPath.StartsWith(normalizedPublicationsPath, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("TargetPath tentando escapar do diretório permitido: {TargetPath} -> {ResolvedPath}", 
                        targetPath, normalizedPath);
                    return null;
                }

                _logger.LogInformation("TargetPath resolvido: {OriginalPath} -> {ResolvedPath}", targetPath, normalizedPath);
                return normalizedPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar targetPath: {TargetPath}", targetPath);
                return null;
            }
        }

        /// <summary>
        /// Atualiza metadados específicos de um deploy (Repository, Branch, BuildCommand, BuildOutput)
        /// </summary>
        /// <param name="name">Nome do deploy para busca</param>
        /// <param name="repository">Novo repository (opcional)</param>
        /// <param name="branch">Nova branch (opcional)</param>
        /// <param name="buildCommand">Novo build command (opcional)</param>
        /// <param name="buildOutput">Novo build output (opcional)</param>
        /// <returns>Resultado da operação de atualização</returns>
        public (bool Success, string Message, DeployMetadata? UpdatedDeploy) UpdateDeployMetadata(
            string name, 
            string? repository = null, 
            string? branch = null, 
            string? buildCommand = null,
            string? buildOutput = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "Nome do deploy é obrigatório", null);
            }

            // Verificar se pelo menos um campo foi fornecido para atualização
            if (string.IsNullOrWhiteSpace(repository) && 
                string.IsNullOrWhiteSpace(branch) && 
                string.IsNullOrWhiteSpace(buildCommand) &&
                string.IsNullOrWhiteSpace(buildOutput))
            {
                return (false, "Pelo menos um campo deve ser fornecido para atualização (Repository, Branch, BuildCommand ou BuildOutput)", null);
            }

            lock (_deploysFileLock)
            {
                try
                {
                    _logger.LogInformation("Atualizando metadados do deploy: {Name}", name);

                    // Carregar metadados existentes
                    var allDeploys = LoadAllDeployMetadata();
                    
                    // Encontrar o deploy pelo nome
                    var deployToUpdate = allDeploys.FirstOrDefault(d => 
                        string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

                    if (deployToUpdate == null)
                    {
                        _logger.LogWarning("Deploy não encontrado para atualização: {Name}", name);
                        return (false, $"Deploy '{name}' não encontrado", null);
                    }

                    // Guardar valores originais para log
                    var originalRepository = deployToUpdate.Repository;
                    var originalBranch = deployToUpdate.Branch;
                    var originalBuildCommand = deployToUpdate.BuildCommand;
                    var originalBuildOutput = deployToUpdate.BuildOutput;

                    // Atualizar apenas os campos fornecidos
                    if (!string.IsNullOrWhiteSpace(repository))
                    {
                        deployToUpdate.Repository = repository.Trim();
                        _logger.LogInformation("Repository atualizado: '{OldValue}' -> '{NewValue}'", 
                            originalRepository, deployToUpdate.Repository);
                    }

                    if (!string.IsNullOrWhiteSpace(branch))
                    {
                        deployToUpdate.Branch = branch.Trim();
                        _logger.LogInformation("Branch atualizada: '{OldValue}' -> '{NewValue}'", 
                            originalBranch, deployToUpdate.Branch);
                    }

                    if (!string.IsNullOrWhiteSpace(buildCommand))
                    {
                        deployToUpdate.BuildCommand = buildCommand.Trim();
                        _logger.LogInformation("BuildCommand atualizado: '{OldValue}' -> '{NewValue}'", 
                            originalBuildCommand, deployToUpdate.BuildCommand);
                    }

                    if (!string.IsNullOrWhiteSpace(buildOutput))
                    {
                        deployToUpdate.BuildOutput = buildOutput.Trim();
                        _logger.LogInformation("BuildOutput atualizado: '{OldValue}' -> '{NewValue}'", 
                            originalBuildOutput, deployToUpdate.BuildOutput);
                    }

                    // Salvar alterações
                    SaveAllDeployMetadata(allDeploys);

                    _logger.LogInformation("Metadados do deploy '{Name}' atualizados com sucesso", name);

                    // Construir mensagem detalhada das alterações
                    var changes = new List<string>();
                    if (!string.IsNullOrWhiteSpace(repository)) 
                        changes.Add($"Repository: '{originalRepository}' → '{deployToUpdate.Repository}'");
                    if (!string.IsNullOrWhiteSpace(branch)) 
                        changes.Add($"Branch: '{originalBranch}' → '{deployToUpdate.Branch}'");
                    if (!string.IsNullOrWhiteSpace(buildCommand)) 
                        changes.Add($"BuildCommand: '{originalBuildCommand}' → '{deployToUpdate.BuildCommand}'");
                    if (!string.IsNullOrWhiteSpace(buildOutput)) 
                        changes.Add($"BuildOutput: '{originalBuildOutput}' → '{deployToUpdate.BuildOutput}'");

                    var detailedMessage = $"Metadados do deploy '{name}' atualizados. Alterações: {string.Join(", ", changes)}";

                    return (true, detailedMessage, deployToUpdate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar metadados do deploy: {Name}", name);
                    return (false, $"Erro interno ao atualizar metadados: {ex.Message}", null);
                }
            }
        }

        /// <summary>
        /// Converte um targetPath do request para um caminho relativo.
        /// Ex: "carteira/api" -> "carteira/api", "C:\temp\wwwroot\carteira\api" -> "carteira/api"
        /// </summary>
        /// <param name="targetPath">TargetPath original do request</param>
        /// <returns>Caminho relativo normalizado</returns>
        private string GetRelativePathFromTargetPath(string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                return string.Empty;
            }

            try
            {
                // Se já é um caminho relativo (como deveria ser do request original)
                if (!Path.IsPathFullyQualified(targetPath))
                {
                    // Normalizar separadores e remover barras iniciais/finais
                    var normalized = targetPath.Replace('\\', '/').Trim('/');
                    return normalized;
                }

                // Se por algum motivo recebeu um caminho completo, extrair a parte relativa
                var normalizedFullPath = Path.GetFullPath(targetPath);
                var normalizedPublicationsPath = Path.GetFullPath(_publicationsPath);

                if (normalizedFullPath.StartsWith(normalizedPublicationsPath, StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = normalizedFullPath.Substring(normalizedPublicationsPath.Length);
                    relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    relativePath = relativePath.Replace('\\', '/');
                    return relativePath;
                }

                // Fallback: retornar apenas o nome da pasta final
                return Path.GetFileName(targetPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao extrair caminho relativo de: {TargetPath}", targetPath);
                return Path.GetFileName(targetPath);
            }
        }

        /// <summary>
        /// Converte um caminho completo para um caminho relativo baseado no _publicationsPath.
        /// Ex: "C:\temp\wwwroot\carteira\api" -> "carteira/api"
        /// </summary>
        /// <param name="fullPath">Caminho completo</param>
        /// <returns>Caminho relativo normalizado</returns>
        private string GetRelativePathFromFullPath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return string.Empty;
            }

            try
            {
                var normalizedFullPath = Path.GetFullPath(fullPath);
                var normalizedPublicationsPath = Path.GetFullPath(_publicationsPath);

                if (normalizedFullPath.StartsWith(normalizedPublicationsPath, StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = normalizedFullPath.Substring(normalizedPublicationsPath.Length);
                    relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    relativePath = relativePath.Replace('\\', '/');
                    return relativePath;
                }

                // Fallback: retornar apenas o nome da pasta final
                return Path.GetFileName(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao extrair caminho relativo de: {FullPath}", fullPath);
                return Path.GetFileName(fullPath);
            }
        }

        /// <summary>
        /// Cria metadados para uma publicação sem executar deploy
        /// </summary>
        /// <param name="name">Nome único do deploy</param>
        /// <param name="repository">URL do repositório</param>
        /// <param name="branch">Branch do repositório</param>
        /// <param name="buildCommand">Comando de build</param>
        /// <param name="buildOutput">Diretório de saída do build</param>
        /// <param name="targetPath">Caminho de destino completo</param>
        /// <returns>Resultado da operação</returns>
        public (bool Success, string Message, DeployMetadata? Metadata) CreateMetadataOnly(
            string name, string repository, string branch, string buildCommand, string buildOutput, string targetPath)
        {
            try
            {
                _logger.LogInformation("Criando metadados apenas: {Name} -> {TargetPath}", name, targetPath);

                if (string.IsNullOrWhiteSpace(name))
                {
                    return (false, "Nome é obrigatório", null);
                }

                if (string.IsNullOrWhiteSpace(repository))
                {
                    return (false, "Repository é obrigatório", null);
                }

                if (string.IsNullOrWhiteSpace(branch))
                {
                    return (false, "Branch é obrigatório", null);
                }

                if (string.IsNullOrWhiteSpace(buildCommand))
                {
                    return (false, "BuildCommand é obrigatório", null);
                }

                if (string.IsNullOrWhiteSpace(buildOutput))
                {
                    return (false, "BuildOutput é obrigatório", null);
                }

                if (string.IsNullOrWhiteSpace(targetPath))
                {
                    return (false, "TargetPath é obrigatório", null);
                }

                var normalizedTargetPath = Path.GetFullPath(targetPath);
                
                var metadata = new DeployMetadata
                {
                    Name = name.Trim(),
                    Repository = repository.Trim(),
                    Branch = branch.Trim(),
                    BuildCommand = buildCommand.Trim(),
                    BuildOutput = buildOutput.Trim(),
                    TargetPath = normalizedTargetPath,
                    DeployedAt = DateTime.MinValue, // Usar MinValue para indicar que nunca foi deployed
                    Exists = Directory.Exists(normalizedTargetPath)
                };

                // Thread-safe operation para ler/escrever o arquivo de metadados
                lock (_deploysFileLock)
                {
                    var deploysList = LoadAllDeployMetadata();
                    
                    // Verificar se já existe metadado com o mesmo nome
                    var existingByName = deploysList.FirstOrDefault(d => 
                        string.Equals(d.Name, metadata.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (existingByName != null)
                    {
                        return (false, $"Já existe um metadado com o nome '{metadata.Name}'", null);
                    }

                    // Verificar se já existe metadado para o mesmo caminho
                    var existingByPath = deploysList.FirstOrDefault(d => 
                        string.Equals(d.TargetPath, normalizedTargetPath, StringComparison.OrdinalIgnoreCase));
                    
                    if (existingByPath != null)
                    {
                        return (false, $"Já existe um metadado para o caminho '{normalizedTargetPath}' (nome: '{existingByPath.Name}')", null);
                    }
                    
                    // Adicionar nova entrada
                    deploysList.Add(metadata);
                    
                    // Salvar de volta
                    SaveAllDeployMetadata(deploysList);
                }

                _logger.LogInformation("Metadados criados com sucesso: {Name} -> {TargetPath}", 
                    metadata.Name, normalizedTargetPath);
                
                return (true, "Metadados criados com sucesso", metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar metadados: {Name}", name);
                return (false, $"Erro interno ao criar metadados: {ex.Message}", null);
            }
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
