using CustomDeploy.Models.Entities;
using CustomDeploy.Data.Repositories;
using CustomDeploy.Data;
using CustomDeploy.Models;
using CustomDeploy.Services;
using Microsoft.EntityFrameworkCore;
using CustomDeploy.Models.DTOs;

namespace CustomDeploy.Services.Business
{
    public class DeployBusinessService : IDeployBusinessService
    {
        private readonly IDeployRepository _deployRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly CustomDeployDbContext _context;
        private readonly DeployService _legacyDeployService; // Serviço original
        private readonly ILogger<DeployBusinessService> _logger;
        private readonly GitHubService _gitHubService; // Adicionado o GitHubService
        private readonly IISManagementService _iisManagementService; // Adicionado IISManagementService

        public DeployBusinessService(
            IDeployRepository deployRepository,
            IUsuarioRepository usuarioRepository,
            CustomDeployDbContext context,
            DeployService legacyDeployService,
            ILogger<DeployBusinessService> logger,
            GitHubService gitHubService,
            IISManagementService iisManagementService) // Injetado IISManagementService
        {
            _deployRepository = deployRepository;
            _usuarioRepository = usuarioRepository;
            _context = context;
            _legacyDeployService = legacyDeployService;
            _logger = logger;
            _gitHubService = gitHubService; // Atribuído o GitHubService
            _iisManagementService = iisManagementService; // Atribuído o IISManagementService
        }

        public async Task<Deploy> CriarDeployAsync(string siteName, string? applicationName, int usuarioId,
            BuildCommand[] BuildCommands, string repoUrl, string branch = "main", string buildOutput = "dist", string? plataforma = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Verificar se o usuário existe
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    throw new InvalidOperationException($"Usuário {usuarioId} não encontrado.");
                }

                // Criar o deploy
                var deploy = new Deploy
                {
                    RepoUrl = repoUrl,
                    Branch = branch,
                    BuildOutput = buildOutput,
                    SiteName = siteName,
                    ApplicationName = applicationName,
                    UsuarioId = usuarioId,
                    Status = "Iniciado",
                    Plataforma = plataforma,
                    Data = DateTime.UtcNow
                };

                

                await _deployRepository.AddAsync(deploy);
                await _deployRepository.SaveChangesAsync();

                // Adicionar comandos
                foreach (var comando in BuildCommands)
                {
                    var deployComando = new DeployComando
                    {
                        DeployId = deploy.Id,
                        Comando = comando.Comando,
                        Ordem = comando.Ordem,
                        TerminalId = comando.TerminalId
                    };

                    _context.DeployComandos.Add(deployComando);
                }

                // Adicionar histórico inicial
                var historicoInicial = new DeployHistorico
                {
                    DeployId = deploy.Id,
                    Status = "Iniciado",
                    Mensagem = "Deploy iniciado",
                    Data = DateTime.UtcNow
                };

                _context.DeployHistoricos.Add(historicoInicial);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Deploy criado com sucesso: {DeployId} para site {SiteName}", 
                    deploy.Id, siteName);

                return deploy;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao criar deploy para site {SiteName}", siteName);
                throw;
            }
        }

        public async Task<Deploy?> ObterDeployPorIdAsync(int id)
        {
            return await _deployRepository.GetByIdAsync(id);
        }

        public async Task<Deploy?> ObterDeployCompletoAsync(int id)
        {
            return await _deployRepository.GetDeployCompleteAsync(id);
        }

        public async Task<IEnumerable<Deploy>> ObterTodosDeploysAsync()
        {
            return await _deployRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Deploy>> ObterDeploysPorSiteAsync(string siteName)
        {
            return await _deployRepository.GetDeploysBySiteNameAsync(siteName);
        }

        public async Task<IEnumerable<Deploy>> ObterDeploysPorUsuarioAsync(int usuarioId)
        {
            return await _deployRepository.GetDeploysByUsuarioAsync(usuarioId);
        }

        public async Task<IEnumerable<Deploy>> ObterDeploysRecentesAsync(int quantidade = 10)
        {
            return await _deployRepository.GetRecentDeploysAsync(quantidade);
        }

        public async Task<bool> AtualizarStatusDeployAsync(int deployId, string status, string? mensagem = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var deploy = await _deployRepository.GetByIdAsync(deployId);
                if (deploy == null)
                {
                    return false;
                }

                // Atualizar o deploy
                deploy.Status = status;
                if (!string.IsNullOrWhiteSpace(mensagem))
                {
                    deploy.Mensagem = mensagem;
                }

                await _deployRepository.UpdateAsync(deploy);

                // Adicionar ao histórico
                var historico = new DeployHistorico
                {
                    DeployId = deployId,
                    Status = status,
                    Mensagem = mensagem,
                    Data = DateTime.UtcNow
                };

                _context.DeployHistoricos.Add(historico);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Status do deploy {DeployId} atualizado para {Status}", 
                    deployId, status);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao atualizar status do deploy {DeployId}", deployId);
                throw;
            }
        }

        public async Task<bool> AdicionarComandoAsync(int deployId, string comando, int ordem, string terminalId = "1")
        {
            try
            {
                var deploy = await _deployRepository.GetByIdAsync(deployId);
                if (deploy == null)
                {
                    return false;
                }

                var deployComando = new DeployComando
                {
                    DeployId = deployId,
                    Comando = comando,
                    Ordem = ordem,
                    TerminalId = terminalId
                };

                _context.DeployComandos.Add(deployComando);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Comando adicionado ao deploy {DeployId}: {Comando}", 
                    deployId, comando);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar comando ao deploy {DeployId}", deployId);
                throw;
            }
        }

        public async Task<bool> AdicionarHistoricoAsync(int deployId, string status, string? mensagem = null)
        {
            try
            {
                var deploy = await _deployRepository.GetByIdAsync(deployId);
                if (deploy == null)
                {
                    return false;
                }

                var historico = new DeployHistorico
                {
                    DeployId = deployId,
                    Status = status,
                    Mensagem = mensagem,
                    Data = DateTime.UtcNow
                };

                _context.DeployHistoricos.Add(historico);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Histórico adicionado ao deploy {DeployId}: {Status}", 
                    deployId, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar histórico ao deploy {DeployId}", deployId);
                throw;
            }
        }

        public async Task<IEnumerable<DeployHistorico>> ObterHistoricoDeployAsync(int deployId)
        {
            var deploy = await _deployRepository.GetDeployWithHistoricoAsync(deployId);
            return deploy?.DeployHistoricos ?? new List<DeployHistorico>();
        }

        public async Task<IEnumerable<DeployComando>> ObterComandosDeployAsync(int deployId)
        {
            var deploy = await _deployRepository.GetDeployWithComandosAsync(deployId);
            return deploy?.DeployComandos ?? new List<DeployComando>();
        }

        public async Task<Deploy> ExecuteDeployCompletoAsync(DeployRequest deployRequest, int usuarioId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogInformation("Iniciando deploy completo para repositório: {RepoUrl} no site: {SiteName}", 
                    deployRequest.RepoUrl, deployRequest.IisSiteName);

                // 1. Verificar se o usuário existe
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    throw new InvalidOperationException($"Usuário {usuarioId} não encontrado.");
                }

                // 2. Determinar nome da aplicação baseado no parsing do IisSiteName
                var (siteName, applicationPath) = ParseSiteAndApplication(deployRequest.IisSiteName, deployRequest.ApplicationPath);
                _logger.LogInformation("Site parseado: {SiteName}, Aplicação: {ApplicationPath}", 
                    siteName, applicationPath ?? "nenhuma");

                // 2.1 Verificar se o site IIS existe e obter informações
                var siteResult = await _iisManagementService.GetSiteInfoAsync(siteName);
                if (!siteResult.Success)
                {
                    throw new InvalidOperationException($"Site não encontrado no IIS: {siteName}. {siteResult.Message}");
                }

                var sitePhysicalPath = siteResult.PhysicalPath;
                _logger.LogInformation("Site IIS encontrado: {SiteName}, Caminho físico: {PhysicalPath}", 
                    siteName, sitePhysicalPath);

                // 2.2 Determinar caminho final baseado na aplicação parseada
                string finalTargetPath;
                if (!string.IsNullOrWhiteSpace(applicationPath))
                {
                    finalTargetPath = Path.Combine(sitePhysicalPath, applicationPath);
                    
                    if (!string.IsNullOrWhiteSpace(deployRequest.TargetPath) && 
                        !string.Equals(deployRequest.TargetPath, applicationPath, StringComparison.OrdinalIgnoreCase))
                    {
                        finalTargetPath = Path.Combine(finalTargetPath, deployRequest.TargetPath);
                        _logger.LogInformation("Combinando caminhos: {ApplicationPath} + {TargetPath}", 
                            applicationPath, deployRequest.TargetPath);
                    }
                }
                else
                {
                    finalTargetPath = Path.Combine(sitePhysicalPath, deployRequest.TargetPath ?? string.Empty);
                }

                // 2.3 Verificar se a aplicação parseada existe no IIS
                if (!string.IsNullOrWhiteSpace(applicationPath))
                {
                    var appResult = await _iisManagementService.CheckApplicationExistsAsync(siteName, applicationPath);
                    if (appResult.Success)
                    {
                        if (appResult.ApplicationExists)
                        {
                            _logger.LogInformation("Aplicação IIS encontrada: {SiteName}/{ApplicationPath}", 
                                siteName, applicationPath);
                        }
                        else
                        {
                            _logger.LogWarning("Aplicação '{ApplicationPath}' não existe como subaplicação no IIS, será tratada como pasta.", 
                                applicationPath);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Erro ao verificar aplicação IIS: {Message}", appResult.Message);
                    }
                }

                // 2.4 Verificar targetPath adicional como aplicação IIS (se diferente da aplicação principal)
                if (!string.IsNullOrWhiteSpace(deployRequest.TargetPath) && deployRequest.TargetPath != applicationPath)
                {
                    var targetCheckPath = !string.IsNullOrWhiteSpace(applicationPath) 
                        ? $"{applicationPath}/{deployRequest.TargetPath}"
                        : deployRequest.TargetPath;
                        
                    var appResult = await _iisManagementService.CheckApplicationExistsAsync(siteName, targetCheckPath);
                    if (appResult.Success && appResult.ApplicationExists)
                    {
                        _logger.LogInformation("Aplicação IIS encontrada no targetPath: {SiteName}/{TargetPath}", 
                            siteName, targetCheckPath);
                    }
                }

                // 3. Criar o deploy no banco ANTES de executar (para ter o ID)
                var deploy = new Deploy
                {
                    RepoUrl = deployRequest.RepoUrl,
                    Branch = deployRequest.Branch,
                    BuildOutput = deployRequest.BuildOutput,
                    SiteName = siteName,
                    ApplicationName = applicationPath,
                    UsuarioId = usuarioId,
                    Status = "Iniciado",
                    Plataforma = deployRequest.Branch, // Usando branch como identificação da plataforma
                    Data = DateTime.UtcNow
                };

                await _deployRepository.AddAsync(deploy);
                await _deployRepository.SaveChangesAsync();

                // 4. Clonar ou atualizar o repositório
                // Determinar o nome do repositório a partir da URL
                var repoName = GetRepoNameFromUrl(deployRequest.RepoUrl);
                string workingDirectory = Path.Combine(Path.GetTempPath(), repoName);
                var cloneResult = await CloneOrUpdateRepositoryAsync(deployRequest.RepoUrl, deployRequest.Branch, workingDirectory);
                if (!cloneResult.Success)
                {
                    throw new InvalidOperationException($"Erro ao clonar ou atualizar o repositório: {cloneResult.Message}");
                }

                // 5. Adicionar comandos do deploy (baseado no BuildCommands do request)
                var comandosBase = deployRequest.BuildCommand?.Select(bc => bc.Comando).ToList() ?? new List<string>();

                // Salvar todos os comandos primeiro
                for (int i = 0; i < comandosBase.Count; i++)
                {
                    var comando = new DeployComando
                    {
                        DeployId = deploy.Id,
                        Comando = comandosBase[i],
                        Ordem = i + 1,
                        Status = "Pendente", // Status inicial
                        Mensagem = "Aguardando execução",
                        TerminalId = deployRequest.BuildCommand?.ElementAtOrDefault(i)?.TerminalId ?? "1" // Use the terminal ID from the request or default to "1"
                    };
                    
                    _context.DeployComandos.Add(comando);
                }

                // 6. Adicionar histórico inicial
                var historicoInicial = new DeployHistorico
                {
                    DeployId = deploy.Id,
                    Status = "Iniciado",
                    Mensagem = $"Deploy iniciado para {deployRequest.RepoUrl} (branch: {deployRequest.Branch}). Total de comandos: {comandosBase.Count}",
                    Data = DateTime.UtcNow
                };

                _context.DeployHistoricos.Add(historicoInicial);
                await _context.SaveChangesAsync();

                // 7. Executar comandos individualmente
                bool deploySuccess = true;
                string deployMessage = "";
                
                try
                {
                    deploySuccess = await ExecutarComandosIndividualmenteAsync(deploy.Id, comandosBase, deployRequest);
                    
                    if (deploySuccess)
                    {
                        deployMessage = "Todos os comandos executados com sucesso";
                        
                        // 8. Executar a parte de cópia de arquivos
                        var copyResult = await CopyBuildOutputToIISPathAsync(workingDirectory, deployRequest.BuildOutput, finalTargetPath);
                        if (!copyResult.Success)
                        {
                            deploySuccess = false;
                            deployMessage = $"Comandos executados, mas falha na cópia de arquivos: {copyResult.Message}";
                        }
                        else
                        {
                            deployMessage = $"Deploy completo realizado com sucesso. {copyResult.Message}";
                        }
                    }
                    else
                    {
                        deployMessage = "Falha na execução de comandos";
                    }
                }
                catch (Exception ex)
                {
                    deploySuccess = false;
                    deployMessage = $"Erro durante execução: {ex.Message}";
                    _logger.LogError(ex, "Erro durante execução dos comandos do deploy {DeployId}", deploy.Id);
                }

                // 9. Atualizar status final baseado no resultado
                string novoStatus = deploySuccess ? "Sucesso" : "Falha";
                
                deploy.Status = novoStatus;
                deploy.Mensagem = deployMessage;
                await _deployRepository.UpdateAsync(deploy);

                // 10. Adicionar histórico final
                var historicoFinal = new DeployHistorico
                {
                    DeployId = deploy.Id,
                    Status = novoStatus,
                    Mensagem = deployMessage,
                    Data = DateTime.UtcNow
                };

                _context.DeployHistoricos.Add(historicoFinal);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Deploy completo {DeployId} finalizado com status: {Status}", 
                    deploy.Id, novoStatus);

                return deploy;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro durante execução do deploy completo");
                throw;
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
        
        private async Task<bool> ExecutarComandosIndividualmenteAsync(int deployId, List<string> comandos, DeployRequest deployRequest)
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), GetRepoNameFromUrl(deployRequest.RepoUrl));

            try
            {
                _logger.LogInformation("Iniciando execução individual de comandos para deploy {DeployId} em {WorkingDir}",
                    deployId, workingDirectory);

                var comandosSalvos = await _context.DeployComandos
                    .Where(dc => dc.DeployId == deployId)
                    .OrderBy(dc => dc.Ordem)
                    .ToListAsync();

                var comandosAgrupadosPorTerminal = comandosSalvos
                    .GroupBy(c => c.TerminalId)
                    .ToList();

                foreach (var grupo in comandosAgrupadosPorTerminal)
                {
                    string? terminalId = grupo.Key;
                    _logger.LogInformation("Executando comandos no terminal: {TerminalId}", terminalId);


                    // Criar um único processo cmd para todos os comandos do mesmo terminal
                    var processInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        WorkingDirectory = workingDirectory,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new System.Diagnostics.Process { StartInfo = processInfo };
                    
                    var outputBuilder = new System.Text.StringBuilder();
                    var errorBuilder = new System.Text.StringBuilder();

                    var outputTcs = new TaskCompletionSource<bool>();
                    var errorTcs = new TaskCompletionSource<bool>();

                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data == null)
                            outputTcs.SetResult(true);
                        else
                        {
                            outputBuilder.AppendLine(args.Data);
                            _logger.LogInformation("Output: {Output}", args.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data == null)
                            errorTcs.SetResult(true);
                        else
                        {
                            errorBuilder.AppendLine(args.Data);
                            _logger.LogError("Error: {Error}", args.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Configurar PATH para npm/yarn se necessário
                    var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                    var nodePaths = $"set PATH={currentPath};C:\\Program Files\\nodejs;C:\\Program Files (x86)\\nodejs;C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\npm\n";
                    await process.StandardInput.WriteLineAsync(nodePaths);
                    await process.StandardInput.FlushAsync();

                    foreach (var comandoBanco in grupo)
                    {
                        string comando = comandoBanco.Comando;
                        _logger.LogInformation("Executando comando: {Comando}", comando);

                        // Atualizar status para "Executando"
                        comandoBanco.Status = "Executando";
                        comandoBanco.Mensagem = "Comando em execução...";
                        comandoBanco.ExecutadoEm = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        try
                        {
                            // Limpar buffers antes de executar novo comando
                            outputBuilder.Clear();
                            errorBuilder.Clear();

                            // Gerar um marker único para identificar o fim do comando
                            var endMarker = $"END_MARKER_{Guid.NewGuid():N}";
                            
                            // Executar o comando com markers e checagem de erro
                            await process.StandardInput.WriteLineAsync(comando);
                            await process.StandardInput.WriteLineAsync($"echo %errorlevel% > \"{workingDirectory}\\{endMarker}\"");
                            await process.StandardInput.FlushAsync();

                            // Aguardar pelo arquivo marker (máximo 1 minuto)
                            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(1));
                            var markerPath = Path.Combine(workingDirectory, endMarker);
                            
                            while (!timeoutTask.IsCompleted)
                            {
                                if (File.Exists(markerPath))
                                {
                                    // Aguardar um pouco mais para garantir que todo output foi capturado
                                    await Task.Delay(1000);
                                    
                                    var errorLevel = await File.ReadAllTextAsync(markerPath);
                                    File.Delete(markerPath); // Limpar o arquivo marker
                                    
                                    var success = errorLevel.Trim() == "0";
                                    var output = outputBuilder.ToString();
                                    var error = errorBuilder.ToString();

                                    if (success)
                                    {
                                        comandoBanco.Status = "Sucesso";
                                        comandoBanco.Mensagem = "Comando executado com sucesso.";
                                        _logger.LogInformation("Comando executado com sucesso: {Comando}", comando);
                                    }
                                    else
                                    {
                                        comandoBanco.Status = "Falha";
                                        var errorMsg = !string.IsNullOrWhiteSpace(error) ? error : output;
                                        comandoBanco.Mensagem = errorMsg;
                                        _logger.LogError("Falha ao executar comando: {Comando}. Erro: {Erro}", comando, errorMsg);
                                        break;
                                    }
                                    
                                    break;
                                }
                                
                                await Task.Delay(100); // Checar a cada 100ms
                            }
                            
                            if (timeoutTask.IsCompleted)
                            {
                                throw new TimeoutException($"O comando excedeu o tempo limite de 10 minutos: {comando}");
                            }
                        }
                        catch (Exception ex)
                        {
                            comandoBanco.Status = "Erro";
                            comandoBanco.Mensagem = ex.Message;
                            _logger.LogError(ex, "Erro inesperado ao executar comando: {Comando}", comando);
                            break;
                        }
                        finally
                        {
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Fechar o shell
                    process.StandardInput.Close();
                    if (!process.WaitForExit(30000)) // 30 segundos de timeout
                    {
                        process.Kill();
                    }
                }

                return true;
            }
            finally
            {
                // Removido Directory.Delete(workingDirectory, true); para reutilização futura do diretório
            }
        }

        private async Task<(bool Success, string Message)> ExecutarComandoAsync(string comando, string workingDirectory)
        {
            try
            {
                // Preparar comando baseado no tipo
                var (fileName, arguments) = PrepararComando(comando);
                
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Adicionar variáveis de ambiente se necessário
                if (comando.StartsWith("npm") || comando.StartsWith("yarn"))
                {
                    var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                    var nodePaths = ";C:\\Program Files\\nodejs;C:\\Program Files (x86)\\nodejs;" +
                                   $"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\npm";
                    processInfo.Environment["PATH"] = currentPath + nodePaths;
                }

                using var process = new System.Diagnostics.Process { StartInfo = processInfo };
                
                var outputBuilder = new System.Text.StringBuilder();
                var errorBuilder = new System.Text.StringBuilder();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        outputBuilder.AppendLine(args.Data);
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        errorBuilder.AppendLine(args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Timeout de 10 minutos por comando
                bool finished = await Task.Run(() => process.WaitForExit(600000));
                
                if (!finished)
                {
                    process.Kill();
                    return (false, "Comando excedeu timeout de 10 minutos");
                }

                var output = outputBuilder.ToString();
                var error = errorBuilder.ToString();

                if (process.ExitCode == 0)
                {
                    return (true, string.IsNullOrWhiteSpace(output) ? "Comando executado com sucesso" : output.Trim());
                }
                else
                {
                    var errorMessage = string.IsNullOrWhiteSpace(error) ? output : error;
                    return (false, $"Exit code {process.ExitCode}: {errorMessage.Trim()}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro na execução: {ex.Message}");
            }
        }

        private (string fileName, string arguments) PrepararComando(string comando)
        {
            comando = comando.Trim();
            
            // Para comandos que precisam do shell (cd, npm, yarn, etc.)
            if (comando.StartsWith("cd ") ||
                comando.StartsWith("dir ") ||
                comando.StartsWith("npm ") || 
                comando.StartsWith("yarn ") || 
                comando.StartsWith("npx ") ||
                comando.StartsWith("cmd ") ||
                comando.Contains("&&") ||
                comando.Contains("&") ||
                comando.Contains("|"))
            {
                return ("cmd.exe", $"/c \"{comando}\"");
            }
            
            // Para comandos diretos (dotnet, git, etc.)
            var parts = comando.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
                return ("cmd.exe", "/c echo No command specified");
            
            if (parts.Length == 1)
            {
                if (parts[0] == "cd" || parts[0] == "dir")
                    return ("cmd.exe", $"/c {parts[0]}");
                return (parts[0], "");
            }
            
            return (parts[0], parts[1]);
        }

        private (string siteName, string? applicationName) ParseSiteAndApplication(string iisSiteName, string? applicationPath)
        {
            // Se applicationPath está especificado, usar ele
            if (!string.IsNullOrWhiteSpace(applicationPath))
            {
                return (iisSiteName, applicationPath);
            }

            // Se IisSiteName contém "/", fazer parse (ex: "gruppy/carteira")
            if (iisSiteName.Contains('/'))
            {
                var parts = iisSiteName.Split('/', 2);
                return (parts[0], parts.Length > 1 ? parts[1] : null);
            }

            // Apenas site, sem aplicação
            return (iisSiteName, null);
        }

        public async Task<bool> ExcluirDeployAsync(int id)
        {
            try
            {
                var result = await _deployRepository.DeleteAsync(id);
                if (result)
                {
                    await _deployRepository.SaveChangesAsync();
                    _logger.LogInformation("Deploy excluído com sucesso: {Id}", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir deploy: {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> ValidateRepositoryAsync(string repoUrl)
        {
            var result = await _gitHubService.ValidateRepositoryAsync(repoUrl);
            return (result.Success, result.Message);
        }

        public async Task<(bool Success, string Message)> ValidateBranchAsync(string repoUrl, string branch)
        {
            var result = await _gitHubService.ValidateBranchAsync(repoUrl, branch);
            return (result.Success, result.Message);
        }

        // public async Task<(bool Success, string Message)> CloneRepositoryAsync(string repoUrl, string branch, string targetPath)
        // {
        //     var result = await _gitHubService.TryCloneWithFallbackAsync(repoUrl, branch, targetPath);
        //     return (result.Success, result.Message);
        // }

        public async Task<(bool Success, string Message)> CloneOrUpdateRepositoryAsync(string repoUrl, string branch, string repoPath)
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
                    // Verificar se a pasta não está vazia
                    if (Directory.EnumerateFileSystemEntries(repoPath).Any())
                    {
                        _logger.LogInformation("Repositório já existe e não está vazio, atualizando: {RepoPath}", repoPath);

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
                        _logger.LogWarning("A pasta do repositório existe, mas está vazia: {RepoPath}", repoPath);

                        // Clonar diretamente na pasta raiz
                        var cloneResult = await RunGitCommandAsync($"clone {authenticatedUrl} .", repoPath);
                        if (!cloneResult.Success)
                        {
                            return (false, $"Falha no clone: {cloneResult.Message}");
                        }

                        return (true, "Repositório clonado diretamente na pasta raiz");
                    }
                }

                _logger.LogInformation("🚀 Clonando repositório com fallback inteligente: {RepoUrl}", repoUrl);
                
                // Usar o novo método de clone com fallback
                var cloneResultFallback = await _gitHubService.TryCloneWithFallbackAsync(repoUrl, branch, repoPath);
                if (!cloneResultFallback.Success)
                {
                    return (false, $"Falha no clone: {cloneResultFallback.Message}");
                }

                var authMethod = cloneResultFallback.UsedSystemCredentials ? "credenciais do sistema" : "credenciais explícitas";
                _logger.LogInformation("✅ Clone realizado com sucesso usando {AuthMethod}", authMethod);
                return (true, $"Repositório clonado com sucesso usando {authMethod}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante operação Git");
                return (false, ex.Message);
            }
        }

        private async Task<(bool Success, string Message)> RunGitCommandAsync(string arguments, string workingDirectory)
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = processInfo };
            
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    outputBuilder.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    errorBuilder.AppendLine(args.Data);
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

        private string GetRepoNameFromUrl(string repoUrl)
        {
            // Extrair nome do repositório da URL
            var uri = new Uri(repoUrl);
            var segments = uri.Segments;
            var lastSegment = segments[segments.Length - 1];
            
            // Remover .git se existir
            if (lastSegment.EndsWith(".git"))
                lastSegment = lastSegment.Substring(0, lastSegment.Length - 4);
            
            return lastSegment;
        }
    }
}
