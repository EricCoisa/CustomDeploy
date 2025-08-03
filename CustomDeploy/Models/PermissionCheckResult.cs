namespace CustomDeploy.Models
{
    /// <summary>
    /// Resultado da verificação de permissões para gerenciamento IIS
    /// </summary>
    public class PermissionCheckResult
    {
        /// <summary>
        /// Indica se a aplicação pode criar diretórios
        /// </summary>
        public bool CanCreateFolders { get; set; }

        /// <summary>
        /// Indica se a aplicação pode mover arquivos
        /// </summary>
        public bool CanMoveFiles { get; set; }

        /// <summary>
        /// Indica se a aplicação pode executar comandos IIS
        /// </summary>
        public bool CanExecuteIISCommands { get; set; }

        /// <summary>
        /// Lista de instruções para resolver problemas de permissão
        /// </summary>
        public List<string> Instructions { get; set; } = new List<string>();

        /// <summary>
        /// Indica se todas as permissões necessárias estão disponíveis
        /// </summary>
        public bool AllPermissionsGranted => CanCreateFolders && CanMoveFiles && CanExecuteIISCommands;

        /// <summary>
        /// Detalhes dos testes realizados
        /// </summary>
        public List<string> TestDetails { get; set; } = new List<string>();
    }
}
