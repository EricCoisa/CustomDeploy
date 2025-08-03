namespace CustomDeploy.Models
{
    /// <summary>
    /// Configurações para autenticação com GitHub
    /// </summary>
    public class GitHubSettings
    {
        /// <summary>
        /// Nome de usuário do GitHub
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Personal Access Token do GitHub
        /// </summary>
        public string PersonalAccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Se deve usar credenciais do sistema (fallback)
        /// </summary>
        public bool UseSystemCredentials { get; set; } = true;

        /// <summary>
        /// Timeout para operações Git (em segundos)
        /// </summary>
        public int GitTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// URL base da API do GitHub (para GitHub Enterprise)
        /// </summary>
        public string ApiBaseUrl { get; set; } = "https://api.github.com";

        /// <summary>
        /// Verificar se as credenciais estão configuradas
        /// </summary>
        public bool HasCredentials => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(PersonalAccessToken);

        /// <summary>
        /// Verificar se deve tentar autenticação explícita
        /// </summary>
        public bool ShouldUseExplicitAuth => HasCredentials && !UseSystemCredentials;
    }
}
