namespace CustomDeploy.Models
{
    public class DeployRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string BuildCommand { get; set; } = string.Empty;
        public string BuildOutput { get; set; } = string.Empty;
        
        /// <summary>
        /// Caminho relativo dentro do diretório de publicações configurado.
        /// Exemplo: "minha-app" será resolvido para "{PublicationsPath}/minha-app"
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;
    }
}
