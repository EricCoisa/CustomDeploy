namespace CustomDeploy.Models
{
    /// <summary>
    /// Request para criação de metadados sem execução de deploy
    /// </summary>
    public class CreateMetadataRequest
    {
        /// <summary>
        /// Nome do site IIS
        /// </summary>
        public string IisSiteName { get; set; } = string.Empty;

        /// <summary>
        /// Subcaminho dentro do site (opcional), como 'api' ou 'app'
        /// </summary>
        public string? SubPath { get; set; }

        /// <summary>
        /// URL do repositório
        /// </summary>
        public string RepoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Branch do repositório
        /// </summary>
        public string Branch { get; set; } = string.Empty;

        /// <summary>
        /// Comando de build
        /// </summary>
        public string BuildCommand { get; set; } = string.Empty;

        /// <summary>
        /// Diretório de saída do build
        /// </summary>
        public string BuildOutput { get; set; } = string.Empty;
    }
}
