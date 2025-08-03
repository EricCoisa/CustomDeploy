using System.Diagnostics;
using System.Text;
using CustomDeploy.Models;
using System.Security.Principal;

namespace CustomDeploy.Services
{
    public class IISManagementService
    {
        private readonly ILogger<IISManagementService> _logger;
        private readonly AdministratorService _administratorService;
        private readonly string _wwwrootPath;

        public IISManagementService(ILogger<IISManagementService> logger, AdministratorService administratorService, IConfiguration configuration)
        {
            _logger = logger;
            _administratorService = administratorService;
            _wwwrootPath = configuration.GetValue<string>("DeploySettings:PublicationsPath") 
                ?? "C:\\inetpub\\wwwroot";
        }

        /// <summary>
        /// Verifica se a aplicação possui as permissões necessárias para gerenciar IIS
        /// </summary>
        /// <returns>Objeto com status das permissões</returns>
        public async Task<(bool Success, string Message, object Permissions)> VerifyPermissionsAsync()
        {
            _logger.LogInformation("Iniciando verificação de permissões do IIS");

            var permissions = new
            {
                canCreateFolders = false,
                canMoveFiles = false,
                canExecuteIISCommands = false,
                details = new List<string>()
            };

            var details = new List<string>();

            try
            {
                // 1. Testar criação de pastas
                var testFolderPath = Path.Combine(_wwwrootPath, "temp-test");
                try
                {
                    if (Directory.Exists(testFolderPath))
                    {
                        Directory.Delete(testFolderPath, true);
                    }

                    Directory.CreateDirectory(testFolderPath);
                    permissions = permissions with { canCreateFolders = true };
                    details.Add("✅ Pode criar pastas no wwwroot");
                    _logger.LogInformation("Teste de criação de pasta: SUCESSO");

                    // Limpar pasta de teste
                    Directory.Delete(testFolderPath, true);
                }
                catch (Exception ex)
                {
                    details.Add($"❌ Não pode criar pastas: {ex.Message}");
                    _logger.LogWarning(ex, "Teste de criação de pasta: FALHA");
                }

                // 2. Testar movimentação de arquivos
                try
                {
                    var tempDir1 = Path.Combine(Path.GetTempPath(), "iis_test_1");
                    var tempDir2 = Path.Combine(Path.GetTempPath(), "iis_test_2");
                    var testFile = Path.Combine(tempDir1, "test.txt");

                    Directory.CreateDirectory(tempDir1);
                    Directory.CreateDirectory(tempDir2);
                    
                    await File.WriteAllTextAsync(testFile, "Test file for IIS permissions");
                    
                    var targetFile = Path.Combine(tempDir2, "test.txt");
                    File.Move(testFile, targetFile);

                    permissions = permissions with { canMoveFiles = true };
                    details.Add("✅ Pode mover arquivos entre pastas");
                    _logger.LogInformation("Teste de movimentação de arquivos: SUCESSO");

                    // Limpeza
                    Directory.Delete(tempDir1, true);
                    Directory.Delete(tempDir2, true);
                }
                catch (Exception ex)
                {
                    details.Add($"❌ Não pode mover arquivos: {ex.Message}");
                    _logger.LogWarning(ex, "Teste de movimentação de arquivos: FALHA");
                }

                // 3. Testar comandos IIS
                try
                {
                    var iisTestResult = await ExecuteCommandAsync("iisreset", "/status");
                    if (iisTestResult.Success)
                    {
                        permissions = permissions with { canExecuteIISCommands = true };
                        details.Add("✅ Pode executar comandos IIS (iisreset /status)");
                        _logger.LogInformation("Teste de comando IIS: SUCESSO");
                    }
                    else
                    {
                        details.Add($"❌ Não pode executar comandos IIS: {iisTestResult.Message}");
                        _logger.LogWarning("Teste de comando IIS: FALHA - {Error}", iisTestResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    details.Add($"❌ Erro ao testar comandos IIS: {ex.Message}");
                    _logger.LogWarning(ex, "Teste de comando IIS: EXCEÇÃO");
                }

                var finalPermissions = permissions with { details = details };

                var allPermissionsOk = finalPermissions.canCreateFolders && 
                                     finalPermissions.canMoveFiles && 
                                     finalPermissions.canExecuteIISCommands;

                var message = allPermissionsOk 
                    ? "Todas as permissões necessárias estão disponíveis"
                    : "Algumas permissões estão faltando. Verifique se a aplicação está executando com privilégios de administrador";

                return (true, message, finalPermissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante verificação de permissões");
                return (false, $"Erro durante verificação: {ex.Message}", permissions with { details = details });
            }
        }

        /// <summary>
        /// Verifica permissões e retorna instruções para resolver problemas
        /// </summary>
        /// <returns>Resultado detalhado da verificação de permissões</returns>
        public async Task<PermissionCheckResult> RequestPermissionsAsync()
        {
            _logger.LogInformation("Iniciando verificação detalhada de permissões com instruções");

            var result = new PermissionCheckResult();
            var appDirectory = AppContext.BaseDirectory;

            try
            {
                // 0. Verificar e solicitar privilégios de administrador
                var (elevationSuccess, elevationMessage) = _administratorService.RequestElevationForCurrentInstance();
                
                if (elevationSuccess)
                {
                    result.TestDetails.Add("✅ Aplicação executando como Administrador");
                    _logger.LogDebug("Verificação de administrador: SUCESSO");
                }
                else
                {
                    result.TestDetails.Add("❌ Aplicação NÃO está executando como Administrador");
                    result.TestDetails.Add($"💡 {elevationMessage}");
                    result.Instructions.Add("🔐 IMPORTANTE: Execute a aplicação como Administrador!");
                    result.Instructions.Add("• Use o endpoint POST /api/iis/request-admin para reiniciar automaticamente");
                    result.Instructions.Add("• Ou feche a aplicação e execute como administrador manualmente");
                    result.Instructions.Add("");
                    _logger.LogWarning("Verificação de administrador: FALHA - {Message}", elevationMessage);
                }
                // 1. Testar criação de diretórios
                _logger.LogDebug("Testando permissão para criar diretórios");
                var testFolderPath = Path.Combine(appDirectory, "temp_permission_test");
                
                try
                {
                    // Limpar pasta de teste se existir
                    if (Directory.Exists(testFolderPath))
                    {
                        Directory.Delete(testFolderPath, true);
                        await Task.Delay(100); // Pequeno delay para garantir limpeza
                    }

                    // Tentar criar diretório
                    Directory.CreateDirectory(testFolderPath);
                    result.CanCreateFolders = true;
                    result.TestDetails.Add("✅ Pode criar diretórios na pasta da aplicação");
                    _logger.LogDebug("Teste de criação de diretório: SUCESSO");

                    // Limpar após teste
                    Directory.Delete(testFolderPath, true);
                }
                catch (UnauthorizedAccessException ex)
                {
                    result.CanCreateFolders = false;
                    result.TestDetails.Add($"❌ Não pode criar diretórios: Acesso negado");
                    result.Instructions.Add("• Execute a aplicação como Administrador (clique com botão direito > 'Executar como administrador')");
                    result.Instructions.Add($"• Verifique permissões da pasta: {appDirectory}");
                    _logger.LogWarning(ex, "Teste de criação de diretório: FALHA - Acesso negado");
                }
                catch (Exception ex)
                {
                    result.CanCreateFolders = false;
                    result.TestDetails.Add($"❌ Erro ao criar diretórios: {ex.Message}");
                    result.Instructions.Add($"• Verifique permissões de escrita na pasta: {appDirectory}");
                    _logger.LogWarning(ex, "Teste de criação de diretório: FALHA");
                }

                // 2. Testar movimentação de arquivos
                _logger.LogDebug("Testando permissão para mover arquivos");
                var tempDir1 = Path.Combine(appDirectory, "temp_move_test_1");
                var tempDir2 = Path.Combine(appDirectory, "temp_move_test_2");
                var testFile = Path.Combine(tempDir1, "test_file.txt");

                try
                {
                    // Limpar diretórios de teste se existirem
                    if (Directory.Exists(tempDir1)) Directory.Delete(tempDir1, true);
                    if (Directory.Exists(tempDir2)) Directory.Delete(tempDir2, true);
                    await Task.Delay(100);

                    // Criar estrutura de teste
                    Directory.CreateDirectory(tempDir1);
                    Directory.CreateDirectory(tempDir2);
                    
                    await File.WriteAllTextAsync(testFile, $"Teste de permissão - {DateTime.Now}");
                    
                    var targetFile = Path.Combine(tempDir2, "test_file.txt");
                    File.Move(testFile, targetFile);

                    result.CanMoveFiles = true;
                    result.TestDetails.Add("✅ Pode mover arquivos entre diretórios");
                    _logger.LogDebug("Teste de movimentação de arquivos: SUCESSO");

                    // Limpeza
                    Directory.Delete(tempDir1, true);
                    Directory.Delete(tempDir2, true);
                }
                catch (UnauthorizedAccessException ex)
                {
                    result.CanMoveFiles = false;
                    result.TestDetails.Add($"❌ Não pode mover arquivos: Acesso negado");
                    result.Instructions.Add("• Execute a aplicação como Administrador");
                    result.Instructions.Add("• Verifique se o antivírus não está bloqueando operações de arquivo");
                    _logger.LogWarning(ex, "Teste de movimentação de arquivos: FALHA - Acesso negado");
                }
                catch (Exception ex)
                {
                    result.CanMoveFiles = false;
                    result.TestDetails.Add($"❌ Erro ao mover arquivos: {ex.Message}");
                    result.Instructions.Add("• Verifique permissões de escrita e leitura na pasta da aplicação");
                    _logger.LogWarning(ex, "Teste de movimentação de arquivos: FALHA");
                }
                finally
                {
                    // Garantir limpeza mesmo em caso de erro
                    try
                    {
                        if (Directory.Exists(tempDir1)) Directory.Delete(tempDir1, true);
                        if (Directory.Exists(tempDir2)) Directory.Delete(tempDir2, true);
                    }
                    catch { /* Ignorar erros de limpeza */ }
                }

                // 3. Testar comandos IIS
                _logger.LogDebug("Testando permissão para executar comandos IIS");
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c iisreset /status",
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
                        result.CanExecuteIISCommands = true;
                        result.TestDetails.Add("✅ Pode executar comandos IIS (iisreset /status)");
                        _logger.LogDebug("Teste de comando IIS: SUCESSO");
                    }
                    else
                    {
                        result.CanExecuteIISCommands = false;
                        result.TestDetails.Add($"❌ Comando IIS falhou (código: {process.ExitCode})");
                        
                        if (error.Contains("Access is denied") || error.Contains("Acesso negado"))
                        {
                            result.Instructions.Add("• Execute a aplicação como Administrador para gerenciar IIS");
                            result.Instructions.Add("• Verifique se o IIS está instalado e em execução");
                        }
                        else
                        {
                            result.Instructions.Add("• Verifique se o IIS está instalado no sistema");
                            result.Instructions.Add("• Instale o módulo 'IIS Management Console' se necessário");
                        }
                        
                        _logger.LogWarning("Teste de comando IIS: FALHA - Código: {ExitCode}, Error: {Error}", 
                            process.ExitCode, error);
                    }
                }
                catch (Exception ex)
                {
                    result.CanExecuteIISCommands = false;
                    result.TestDetails.Add($"❌ Erro ao executar comando IIS: {ex.Message}");
                    result.Instructions.Add("• Execute a aplicação como Administrador");
                    result.Instructions.Add("• Verifique se o IIS está instalado e configurado");
                    result.Instructions.Add("• Certifique-se de que o Windows Feature 'IIS Management Console' está habilitado");
                    _logger.LogWarning(ex, "Teste de comando IIS: EXCEÇÃO");
                }

                // 4. Adicionar instruções gerais se houver falhas
                if (!result.AllPermissionsGranted)
                {
                    result.Instructions.Add("");
                    result.Instructions.Add("📋 Instruções Gerais:");
                    result.Instructions.Add("• Feche a aplicação completamente");
                    result.Instructions.Add("• Clique com botão direito no executável > 'Executar como administrador'");
                    result.Instructions.Add("• Verifique se o IIS está instalado (Control Panel > Programs > Turn Windows features on/off > IIS)");
                    result.Instructions.Add("• Desabilite temporariamente o antivírus se necessário");
                }
                else
                {
                    result.Instructions.Add("✅ Todas as permissões necessárias estão disponíveis!");
                    result.Instructions.Add("A aplicação pode gerenciar sites IIS sem problemas.");
                }

                _logger.LogInformation("Verificação de permissões concluída. CanCreateFolders: {CanCreateFolders}, CanMoveFiles: {CanMoveFiles}, CanExecuteIIS: {CanExecuteIISCommands}", 
                    result.CanCreateFolders, result.CanMoveFiles, result.CanExecuteIISCommands);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante verificação detalhada de permissões");
                
                result.CanCreateFolders = false;
                result.CanMoveFiles = false;
                result.CanExecuteIISCommands = false;
                result.TestDetails.Add($"❌ Erro crítico durante verificação: {ex.Message}");
                result.Instructions.Add("• Execute a aplicação como Administrador");
                result.Instructions.Add("• Verifique se há conflitos com antivírus ou firewall");
                result.Instructions.Add("• Contate o suporte técnico se o problema persistir");

                return result;
            }
        }

        /// <summary>
        /// Cria um novo site no IIS com AppPool associado
        /// </summary>
        /// <param name="siteName">Nome do site</param>
        /// <param name="port">Porta do site</param>
        /// <param name="physicalPath">Caminho físico do site</param>
        /// <param name="appPool">Nome do Application Pool</param>
        /// <returns>Resultado da operação</returns>
        public async Task<(bool Success, string Message, object? Details)> CreateSiteAsync(
            string siteName, 
            int port, 
            string physicalPath, 
            string appPool)
        {
            _logger.LogInformation("Iniciando criação do site IIS: {SiteName}, Porta: {Port}, Path: {PhysicalPath}, AppPool: {AppPool}", 
                siteName, port, physicalPath, appPool);

            var operationLog = new List<string>();

            try
            {
                // Validações básicas
                if (string.IsNullOrWhiteSpace(siteName))
                {
                    return (false, "Nome do site é obrigatório", null);
                }

                if (port <= 0 || port > 65535)
                {
                    return (false, "Porta deve estar entre 1 e 65535", null);
                }

                if (string.IsNullOrWhiteSpace(physicalPath))
                {
                    return (false, "Caminho físico é obrigatório", null);
                }

                if (string.IsNullOrWhiteSpace(appPool))
                {
                    return (false, "Nome do Application Pool é obrigatório", null);
                }

                // Verificar se o diretório físico existe
                if (!Directory.Exists(physicalPath))
                {
                    _logger.LogInformation("Diretório não existe, criando: {PhysicalPath}", physicalPath);
                    try
                    {
                        Directory.CreateDirectory(physicalPath);
                        operationLog.Add($"✅ Diretório criado: {physicalPath}");
                    }
                    catch (Exception ex)
                    {
                        operationLog.Add($"❌ Erro ao criar diretório: {ex.Message}");
                        return (false, $"Não foi possível criar o diretório: {ex.Message}", 
                            new { operationLog });
                    }
                }
                else
                {
                    operationLog.Add($"✅ Diretório já existe: {physicalPath}");
                }

                // 1. Verificar se o AppPool já existe
                var appPoolExistsResult = await ExecuteCommandAsync("powershell", 
                    $"-Command \"Get-IISAppPool -Name '{appPool}' -ErrorAction SilentlyContinue\"");

                if (!appPoolExistsResult.Success || string.IsNullOrWhiteSpace(appPoolExistsResult.Output))
                {
                    // 2. Criar Application Pool
                    _logger.LogInformation("Criando Application Pool: {AppPool}", appPool);
                    var createAppPoolResult = await ExecuteCommandAsync("powershell", 
                        $"-Command \"New-IISAppPool -Name '{appPool}' -Force\"");

                    if (createAppPoolResult.Success)
                    {
                        operationLog.Add($"✅ Application Pool criado: {appPool}");
                        _logger.LogInformation("Application Pool criado com sucesso: {AppPool}", appPool);
                    }
                    else
                    {
                        operationLog.Add($"❌ Erro ao criar Application Pool: {createAppPoolResult.Message}");
                        return (false, $"Falha ao criar Application Pool: {createAppPoolResult.Message}", 
                            new { operationLog });
                    }
                }
                else
                {
                    operationLog.Add($"✅ Application Pool já existe: {appPool}");
                    _logger.LogInformation("Application Pool já existe: {AppPool}", appPool);
                }

                // 3. Verificar se a porta já está em uso
                var portCheckResult = await ExecuteCommandAsync("powershell", 
                    $"-Command \"Get-IISSite | Where-Object {{ $_.Bindings.bindingInformation -like '*:{port}:*' }}\"");

                if (portCheckResult.Success && !string.IsNullOrWhiteSpace(portCheckResult.Output))
                {
                    operationLog.Add($"❌ Porta {port} já está em uso por outro site");
                    return (false, $"Porta {port} já está em uso por outro site", new { operationLog });
                }

                // 4. Verificar se o site já existe
                var siteExistsResult = await ExecuteCommandAsync("powershell", 
                    $"-Command \"Get-IISSite -Name '{siteName}' -ErrorAction SilentlyContinue\"");

                if (siteExistsResult.Success && !string.IsNullOrWhiteSpace(siteExistsResult.Output))
                {
                    operationLog.Add($"❌ Site já existe: {siteName}");
                    return (false, $"Site '{siteName}' já existe", new { operationLog });
                }

                // 5. Criar o site
                _logger.LogInformation("Criando site IIS: {SiteName}", siteName);
                var createSiteResult = await ExecuteCommandAsync("powershell", 
                    $"-Command \"New-IISSite -Name '{siteName}' -PhysicalPath '{physicalPath}' -Port {port} -ApplicationPool '{appPool}'\"");

                if (createSiteResult.Success)
                {
                    operationLog.Add($"✅ Site criado com sucesso: {siteName}");
                    _logger.LogInformation("Site IIS criado com sucesso: {SiteName}", siteName);

                    var details = new
                    {
                        siteName,
                        port,
                        physicalPath,
                        appPool,
                        operationLog,
                        createdAt = DateTime.UtcNow
                    };

                    return (true, $"Site '{siteName}' criado com sucesso na porta {port}", details);
                }
                else
                {
                    operationLog.Add($"❌ Erro ao criar site: {createSiteResult.Message}");
                    return (false, $"Falha ao criar site: {createSiteResult.Message}", new { operationLog });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante criação do site IIS: {SiteName}", siteName);
                operationLog.Add($"❌ Exceção durante criação: {ex.Message}");
                return (false, $"Erro interno durante criação do site: {ex.Message}", new { operationLog });
            }
        }

        /// <summary>
        /// Executa um comando no sistema operacional
        /// </summary>
        /// <param name="fileName">Nome do executável</param>
        /// <param name="arguments">Argumentos do comando</param>
        /// <returns>Resultado da execução</returns>
        private async Task<(bool Success, string Message, string Output)> ExecuteCommandAsync(string fileName, string arguments)
        {
            try
            {
                _logger.LogDebug("Executando comando: {FileName} {Arguments}", fileName, arguments);

                var processInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
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

                _logger.LogDebug("Comando executado. ExitCode: {ExitCode}, Output: {Output}, Error: {Error}", 
                    process.ExitCode, output, error);

                if (process.ExitCode == 0)
                {
                    return (true, "Comando executado com sucesso", output);
                }
                else
                {
                    var errorMessage = !string.IsNullOrEmpty(error) ? error : output;
                    return (false, $"Comando falhou (código: {process.ExitCode}): {errorMessage}", output);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comando: {FileName} {Arguments}", fileName, arguments);
                return (false, ex.Message, string.Empty);
            }
        }
    }
}
