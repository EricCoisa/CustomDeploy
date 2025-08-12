using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace CustomDeploy.Services
{
    /// <summary>
    /// Serviço para gerenciamento de privilégios de administrador
    /// </summary>
    public class AdministratorService
    {
        private readonly ILogger<AdministratorService> _logger;

        public AdministratorService(ILogger<AdministratorService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Verifica se a aplicação está executando com privilégios de administrador
        /// </summary>
        /// <returns>True se for administrador, False caso contrário</returns>
        public bool IsRunningAsAdministrator()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    _logger.LogWarning("Verificação de administrador disponível apenas no Windows");
                    return false;
                }

                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                
                _logger.LogDebug("Verificação de privilégios de administrador: {IsAdmin}", isAdmin);
                return isAdmin;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao verificar privilégios de administrador");
                return false;
            }
        }

        /// <summary>
        /// Tenta reiniciar a aplicação com privilégios de administrador
        /// </summary>
        /// <returns>Tuple indicando sucesso e mensagem</returns>
        public (bool Success, string Message) RequestAdministratorRestart()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    var message = "Elevação de privilégios disponível apenas no Windows";
                    _logger.LogWarning(message);
                    return (false, message);
                }

                if (IsRunningAsAdministrator())
                {
                    return (true, "Aplicação já está executando com privilégios de administrador");
                }

                var currentProcess = Process.GetCurrentProcess();
                var executablePath = currentProcess.MainModule?.FileName ?? Environment.ProcessPath;
                
                if (string.IsNullOrEmpty(executablePath))
                {
                    return (false, "Não foi possível determinar o caminho do executável");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = true,
                    Verb = "runas", // Solicita elevação
                    Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).Select(arg => $"\"{arg}\""))
                };

                _logger.LogInformation("Solicitando reinicialização como administrador: {ExecutablePath}", executablePath);

                Process.Start(startInfo);
                
                // Agenda o fechamento da aplicação atual
                Task.Delay(2000).ContinueWith(_ => Environment.Exit(0));
                
                return (true, "Solicitação de elevação enviada. A aplicação será reiniciada com privilégios de administrador.");
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // Usuário cancelou o UAC (User Account Control)
                _logger.LogWarning("Usuário cancelou a solicitação de elevação de privilégios");
                return (false, "Operação cancelada pelo usuário. Privilégios de administrador são necessários para gerenciar IIS.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar reiniciar como administrador");
                return (false, $"Erro ao tentar reiniciar como administrador: {ex.Message}");
            }
        }

        /// <summary>
        /// Solicita elevação de privilégios para a instância atual sem reiniciar
        /// </summary>
        /// <returns>Tuple indicando sucesso e mensagem</returns>
        public (bool Success, string Message) RequestElevationForCurrentInstance()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    var message = "Elevação de privilégios disponível apenas no Windows";
                    _logger.LogWarning(message);
                    return (false, message);
                }

                if (IsRunningAsAdministrator())
                {
                    return (true, "Aplicação já está executando com privilégios de administrador");
                }

                _logger.LogWarning("ATENÇÃO: Para gerenciar IIS é necessário privilégios de administrador");
                _logger.LogInformation("A aplicação atual não possui privilégios de administrador");
                
                return (false, "Para utilizar funcionalidades IIS, é necessário executar a aplicação como Administrador. Use o endpoint /iis/request-admin para reiniciar automaticamente ou execute manualmente como administrador.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar privilégios de administrador");
                return (false, $"Erro ao verificar privilégios: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém instruções detalhadas para executar como administrador
        /// </summary>
        /// <returns>Lista de instruções passo-a-passo</returns>
        public List<string> GetAdministratorInstructions()
        {
            var instructions = new List<string>();

            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var executablePath = currentProcess.MainModule?.FileName ?? "aplicação";
                var executableName = Path.GetFileName(executablePath);

                instructions.Add("🔐 Para executar como Administrador:");
                instructions.Add("");
                instructions.Add("📋 Método 1 - Reiniciar Automaticamente:");
                instructions.Add("• Use o endpoint POST /iis/request-admin para reiniciar automaticamente");
                instructions.Add("• A aplicação solicitará permissões UAC e reiniciará");
                instructions.Add("");
                instructions.Add("📋 Método 2 - Manual:");
                instructions.Add("• 1. Feche esta aplicação completamente");
                instructions.Add($"• 2. Localize o arquivo: {executableName}");
                instructions.Add("• 3. Clique com o botão DIREITO no arquivo");
                instructions.Add("• 4. Selecione 'Executar como administrador'");
                instructions.Add("• 5. Clique 'Sim' quando aparecer o controle de conta de usuário (UAC)");
                instructions.Add("");
                instructions.Add("📋 Método 3 - Via Prompt de Comando:");
                instructions.Add("• 1. Abra o Prompt de Comando como Administrador");
                instructions.Add("• 2. Navegue até a pasta da aplicação");
                instructions.Add($"• 3. Execute: {executableName}");
                instructions.Add("");
                instructions.Add("⚠️ Importante:");
                instructions.Add("• Privilégios de administrador são OBRIGATÓRIOS para gerenciar IIS");
                instructions.Add("• A aplicação não funcionará corretamente sem esses privilégios");
                instructions.Add("• Esta verificação garante que todas as operações IIS funcionem adequadamente");

                _logger.LogDebug("Instruções de administrador geradas para: {ExecutablePath}", executablePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao gerar instruções de administrador");
                instructions.Add("❌ Erro ao gerar instruções específicas");
                instructions.Add("• Execute a aplicação como Administrador");
                instructions.Add("• Clique com botão direito > 'Executar como administrador'");
            }

            return instructions;
        }

        /// <summary>
        /// Verifica privilégios e retorna status detalhado
        /// </summary>
        /// <returns>Informações sobre privilégios e instruções</returns>
        public (bool IsAdmin, string UserName, string Domain, List<string> Instructions) GetPrivilegeStatus()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    _logger.LogWarning("Verificação de privilégios disponível apenas no Windows");
                    return (false, "N/A", "N/A", new List<string> { "❌ Funcionalidade disponível apenas no Windows" });
                }

                var isAdmin = IsRunningAsAdministrator();
                using var identity = WindowsIdentity.GetCurrent();
                var userName = identity.Name ?? "Usuário desconhecido";
                
                // Separar domínio e usuário
                var userParts = userName.Split('\\');
                var domain = userParts.Length > 1 ? userParts[0] : Environment.MachineName;
                var user = userParts.Length > 1 ? userParts[1] : userName;

                var instructions = isAdmin 
                    ? new List<string> { "✅ Aplicação executando com privilégios de administrador", "Todas as operações IIS estão disponíveis" }
                    : GetAdministratorInstructions();

                _logger.LogInformation("Status de privilégios: IsAdmin={IsAdmin}, User={UserName}", isAdmin, userName);

                return (isAdmin, user, domain, instructions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status de privilégios");
                return (false, "Erro", "Erro", new List<string> { $"❌ Erro ao verificar privilégios: {ex.Message}" });
            }
        }
    }
}
