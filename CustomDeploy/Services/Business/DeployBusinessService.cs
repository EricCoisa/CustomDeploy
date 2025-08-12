using CustomDeploy.Models.Entities;
using CustomDeploy.Data.Repositories;
using CustomDeploy.Data;
using CustomDeploy.Models;
using CustomDeploy.Services;
using Microsoft.EntityFrameworkCore;

namespace CustomDeploy.Services.Business
{
    public class DeployBusinessService : IDeployBusinessService
    {
        private readonly IDeployRepository _deployRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly CustomDeployDbContext _context;
        private readonly DeployService _legacyDeployService; // Serviço original
        private readonly ILogger<DeployBusinessService> _logger;

        public DeployBusinessService(
            IDeployRepository deployRepository,
            IUsuarioRepository usuarioRepository,
            CustomDeployDbContext context,
            DeployService legacyDeployService,
            ILogger<DeployBusinessService> logger)
        {
            _deployRepository = deployRepository;
            _usuarioRepository = usuarioRepository;
            _context = context;
            _legacyDeployService = legacyDeployService;
            _logger = logger;
        }

        public async Task<Deploy> CriarDeployAsync(string siteName, string? applicationName, int usuarioId, 
            string[] comandos, string? plataforma = null)
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
                for (int i = 0; i < comandos.Length; i++)
                {
                    var comando = new DeployComando
                    {
                        DeployId = deploy.Id,
                        Comando = comandos[i],
                        Ordem = i + 1
                    };
                    
                    _context.DeployComandos.Add(comando);
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

        public async Task<bool> AdicionarComandoAsync(int deployId, string comando, int ordem)
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
                    Ordem = ordem
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
                var (siteName, applicationName) = ParseSiteAndApplication(deployRequest.IisSiteName, deployRequest.ApplicationPath);
                
                // 3. Criar o deploy no banco ANTES de executar (para ter o ID)
                var deploy = new Deploy
                {
                    SiteName = siteName,
                    ApplicationName = applicationName,
                    UsuarioId = usuarioId,
                    Status = "Iniciado",
                    Plataforma = deployRequest.Branch, // Usando branch como identificação da plataforma
                    Data = DateTime.UtcNow
                };

                await _deployRepository.AddAsync(deploy);
                await _deployRepository.SaveChangesAsync();

                // 4. Adicionar comandos do deploy (baseado no BuildCommands do request)
                var comandosBase = new List<string>
                {
                    $"git clone {deployRequest.RepoUrl}",
                    $"git checkout {deployRequest.Branch}"
                };

                // Adicionar os comandos de build fornecidos no request
                if (deployRequest.BuildCommands != null && deployRequest.BuildCommands.Length > 0)
                {
                    comandosBase.AddRange(deployRequest.BuildCommands);
                }

                // Salvar todos os comandos primeiro
                for (int i = 0; i < comandosBase.Count; i++)
                {
                    var comando = new DeployComando
                    {
                        DeployId = deploy.Id,
                        Comando = comandosBase[i],
                        Ordem = i + 1,
                        Status = "Pendente", // Status inicial
                        Mensagem = "Aguardando execução"
                    };
                    
                    _context.DeployComandos.Add(comando);
                }

                // 5. Adicionar histórico inicial
                var historicoInicial = new DeployHistorico
                {
                    DeployId = deploy.Id,
                    Status = "Iniciado",
                    Mensagem = $"Deploy iniciado para {deployRequest.RepoUrl} (branch: {deployRequest.Branch}). Total de comandos: {comandosBase.Count}",
                    Data = DateTime.UtcNow
                };

                _context.DeployHistoricos.Add(historicoInicial);
                await _context.SaveChangesAsync();

                // 6. Executar comandos individualmente
                bool deploySuccess = true;
                string deployMessage = "";
                
                try
                {
                    deploySuccess = await ExecutarComandosIndividualmenteAsync(deploy.Id, comandosBase, deployRequest);
                    
                    if (deploySuccess)
                    {
                        deployMessage = "Todos os comandos executados com sucesso";
                        
                        // 7. Executar a parte de cópia de arquivos usando o serviço legacy
                        var copyResult = await _legacyDeployService.ExecuteDeployAsync(deployRequest);
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

                // 8. Atualizar status final baseado no resultado
                string novoStatus = deploySuccess ? "Sucesso" : "Falha";
                
                deploy.Status = novoStatus;
                deploy.Mensagem = deployMessage;
                await _deployRepository.UpdateAsync(deploy);

                // 9. Adicionar histórico final
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

        private async Task<bool> ExecutarComandosIndividualmenteAsync(int deployId, List<string> comandos, DeployRequest deployRequest)
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), $"deploy_{deployId}_{DateTime.Now:yyyyMMddHHmmss}");
            
            try
            {
                // Criar diretório de trabalho
                Directory.CreateDirectory(workingDirectory);
                
                _logger.LogInformation("Iniciando execução individual de comandos para deploy {DeployId} em {WorkingDir}", 
                    deployId, workingDirectory);

                // Buscar comandos salvos no banco
                var comandosSalvos = await _context.DeployComandos
                    .Where(dc => dc.DeployId == deployId)
                    .OrderBy(dc => dc.Ordem)
                    .ToListAsync();

                bool todosComandosOk = true;

                for (int i = 0; i < comandosSalvos.Count; i++)
                {
                    var comandoBanco = comandosSalvos[i];
                    string comando = comandoBanco.Comando;
                    
                    _logger.LogInformation("Executando comando {Ordem}/{Total}: {Comando}", 
                        i + 1, comandosSalvos.Count, comando);

                    // Atualizar status para "Executando"
                    comandoBanco.Status = "Executando";
                    comandoBanco.Mensagem = "Comando em execução...";
                    comandoBanco.ExecutadoEm = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Adicionar histórico
                    var historicoComando = new DeployHistorico
                    {
                        DeployId = deployId,
                        Status = "Executando",
                        Mensagem = $"Executando comando {i + 1}: {comando}",
                        Data = DateTime.UtcNow
                    };
                    _context.DeployHistoricos.Add(historicoComando);
                    await _context.SaveChangesAsync();

                    try
                    {
                        // Executar o comando
                        var resultado = await ExecutarComandoAsync(comando, workingDirectory);
                        
                        if (resultado.Success)
                        {
                            comandoBanco.Status = "Sucesso";
                            comandoBanco.Mensagem = $"Comando executado com sucesso. {resultado.Message}";
                            
                            // Log de sucesso
                            _logger.LogInformation("Comando {Ordem} executado com sucesso: {Comando}", 
                                i + 1, comando);
                                
                            // Histórico de sucesso
                            var historicoSucesso = new DeployHistorico
                            {
                                DeployId = deployId,
                                Status = "ComandoOK",
                                Mensagem = $"Comando {i + 1} executado: {resultado.Message}",
                                Data = DateTime.UtcNow
                            };
                            _context.DeployHistoricos.Add(historicoSucesso);
                        }
                        else
                        {
                            comandoBanco.Status = "Falha";
                            comandoBanco.Mensagem = $"Comando falhou: {resultado.Message}";
                            todosComandosOk = false;
                            
                            // Log de erro
                            _logger.LogError("Comando {Ordem} falhou: {Comando}. Erro: {Erro}", 
                                i + 1, comando, resultado.Message);
                                
                            // Histórico de erro
                            var historicoErro = new DeployHistorico
                            {
                                DeployId = deployId,
                                Status = "ComandoFalha",
                                Mensagem = $"Comando {i + 1} falhou: {resultado.Message}",
                                Data = DateTime.UtcNow
                            };
                            _context.DeployHistoricos.Add(historicoErro);
                            
                            // Parar execução se um comando falhar
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        comandoBanco.Status = "Erro";
                        comandoBanco.Mensagem = $"Erro inesperado: {ex.Message}";
                        todosComandosOk = false;
                        
                        _logger.LogError(ex, "Erro inesperado ao executar comando {Ordem}: {Comando}", 
                            i + 1, comando);
                            
                        // Histórico de erro
                        var historicoErro = new DeployHistorico
                        {
                            DeployId = deployId,
                            Status = "ComandoErro",
                            Mensagem = $"Erro no comando {i + 1}: {ex.Message}",
                            Data = DateTime.UtcNow
                        };
                        _context.DeployHistoricos.Add(historicoErro);
                        
                        break;
                    }
                    finally
                    {
                        // Salvar status do comando
                        await _context.SaveChangesAsync();
                    }
                }

                return todosComandosOk;
            }
            finally
            {
                // Limpar diretório temporário
                try
                {
                    if (Directory.Exists(workingDirectory))
                    {
                        Directory.Delete(workingDirectory, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao limpar diretório temporário: {WorkingDir}", workingDirectory);
                }
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
            
            if (comando.StartsWith("git "))
            {
                return ("git", comando.Substring(4));
            }
            else if (comando.StartsWith("npm "))
            {
                return ("npm", comando.Substring(4));
            }
            else if (comando.StartsWith("yarn "))
            {
                return ("yarn", comando.Substring(5));
            }
            else if (comando.StartsWith("dotnet "))
            {
                return ("dotnet", comando.Substring(7));
            }
            else if (comando.StartsWith("node "))
            {
                return ("node", comando.Substring(5));
            }
            else
            {
                // Para comandos genéricos, usar cmd
                return ("cmd", $"/c {comando}");
            }
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
    }
}
