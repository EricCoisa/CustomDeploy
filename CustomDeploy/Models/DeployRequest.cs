namespace CustomDeploy.Models
{
    public class DeployRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string BuildCommand { get; set; } = string.Empty;
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
}
