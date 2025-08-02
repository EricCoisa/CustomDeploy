using CustomDeploy.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CustomDeploy.Services
{
    public class DeployService
    {
        private readonly ILogger<DeployService> _logger;
        private readonly string _workingDirectory;
        private readonly string _publicationsPath;

        public DeployService(ILogger<DeployService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _workingDirectory = configuration.GetValue<string>("DeploySettings:WorkingDirectory") 
                ?? Path.Combine(Path.GetTempPath(), "CustomDeploy");
            _publicationsPath = configuration.GetValue<string>("DeploySettings:PublicationsPath") 
                ?? "C:\\temp\\wwwroot";
            
            // Criar diretório de trabalho se não existir
            if (!Directory.Exists(_workingDirectory))
            {
                Directory.CreateDirectory(_workingDirectory);
            }
        }

        public async Task<(bool Success, string Message)> ExecuteDeployAsync(DeployRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var repoName = GetRepoNameFromUrl(request.RepoUrl);
            var repoPath = Path.Combine(_workingDirectory, repoName);

            try
            {
                _logger.LogInformation("Iniciando deploy para repositório: {RepoUrl}", request.RepoUrl);

                // 0. Validar e resolver o targetPath
                var resolvedTargetPath = ValidateAndResolveTargetPath(request.TargetPath);
                if (resolvedTargetPath == null)
                {
                    return (false, $"TargetPath inválido. Deve ser um caminho relativo dentro de {_publicationsPath}");
                }

                // Atualizar o request com o caminho resolvido
                request.TargetPath = resolvedTargetPath;

                // 1. Clonar ou atualizar repositório
                var gitResult = await CloneOrUpdateRepositoryAsync(request.RepoUrl, request.Branch, repoPath);
                if (!gitResult.Success)
                {
                    return (false, $"Erro no Git: {gitResult.Message}");
                }

                // 2. Executar comando de build
                var buildResult = await ExecuteBuildCommandAsync(request.BuildCommand, repoPath);
                if (!buildResult.Success)
                {
                    return (false, $"Erro no build: {buildResult.Message}");
                }

                // 3. Copiar arquivos para destino
                var copyResult = await CopyBuildOutputAsync(repoPath, request.BuildOutput, request.TargetPath);
                if (!copyResult.Success)
                {
                    return (false, $"Erro na cópia: {copyResult.Message}");
                }

                // 4. Salvar metadados do deploy
                var metadataResult = await SaveDeployMetadataAsync(request);
                if (!metadataResult.Success)
                {
                    _logger.LogWarning("Falha ao salvar metadados: {Message}", metadataResult.Message);
                    // Não falha o deploy por causa dos metadados
                }

                stopwatch.Stop();
                var successMessage = $"Deploy concluído com sucesso em {stopwatch.Elapsed.TotalSeconds:F2} segundos";
                _logger.LogInformation(successMessage);
                
                return (true, successMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erro durante o deploy");
                return (false, $"Erro interno: {ex.Message}");
            }
        }

        private async Task<(bool Success, string Message)> CloneOrUpdateRepositoryAsync(string repoUrl, string branch, string repoPath)
        {
            try
            {
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
                    _logger.LogInformation("Clonando repositório: {RepoUrl}", repoUrl);
                    
                    var cloneResult = await RunGitCommandAsync($"clone -b {branch} {repoUrl} \"{repoPath}\"", _workingDirectory);
                    if (!cloneResult.Success)
                    {
                        return (false, $"Falha no clone: {cloneResult.Message}");
                    }

                    return (true, "Repositório clonado com sucesso");
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
            // Para comandos que precisam do shell (npm, yarn, etc.)
            if (command.StartsWith("npm ") || 
                command.StartsWith("yarn ") || 
                command.StartsWith("npx ") ||
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

        private async Task<(bool Success, string Message)> SaveDeployMetadataAsync(DeployRequest request)
        {
            try
            {
                var metadata = new DeployMetadata
                {
                    Repository = request.RepoUrl,
                    Branch = request.Branch,
                    BuildCommand = request.BuildCommand,
                    DeployedAt = DateTime.UtcNow
                };

                var metadataPath = Path.Combine(request.TargetPath, "deploy.json");
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonContent = JsonSerializer.Serialize(metadata, jsonOptions);
                await File.WriteAllTextAsync(metadataPath, jsonContent, Encoding.UTF8);

                _logger.LogInformation("Metadados do deploy salvos em: {MetadataPath}", metadataPath);
                return (true, "Metadados salvos com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar metadados do deploy");
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
    }
}
