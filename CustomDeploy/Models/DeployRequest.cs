namespace CustomDeploy.Models
{
    public class DeployRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string[] BuildCommands { get; set; } = Array.Empty<string>();
        public string BuildOutput { get; set; } = string.Empty;
        
        /// <summary>
        /// Caminho relativo dentro do site IIS.
        /// Exemplo: "api" será resolvido para "{SitePhysicalPath}/api"
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome do site IIS onde o projeto será publicado.
        /// Pode conter "/" para separar site/aplicação (ex: "gruppy/carteira")
        /// ou ser usado junto com ApplicationPath
        /// </summary>
        public string IisSiteName { get; set; } = string.Empty;
        
        /// <summary>
        /// Caminho da aplicação IIS (opcional).
        /// Se especificado, será usado junto com IisSiteName.
        /// Se não especificado, IisSiteName pode conter "site/aplicacao"
        /// </summary>
        public string? ApplicationPath { get; set; }
    }

    /// <summary>
    /// Request para testar credenciais Git
    /// </summary>
    public class TestCredentialsRequest
    {
        /// <summary>
        /// URL do repositório para teste (opcional)
        /// Se não especificado, usa repositório público padrão
        /// </summary>
        public string? RepoUrl { get; set; }
    }

    /// <summary>
    /// Request para validar repositório e branch
    /// </summary>
    public class ValidateRepositoryRequest
    {
        /// <summary>
        /// URL do repositório Git
        /// </summary>
        public string RepoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Nome da branch para validação (opcional)
        /// </summary>
        public string? Branch { get; set; }
    }

    /// <summary>
    /// Request para validar branch específica
    /// </summary>
    public class ValidateBranchRequest
    {
        /// <summary>
        /// URL do repositório Git
        /// </summary>
        public string RepoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Nome da branch para validação
        /// </summary>
        public string Branch { get; set; } = string.Empty;
    }
}
