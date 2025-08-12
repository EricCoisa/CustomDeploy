namespace CustomDeploy.Models
{
    public class PublicationInfo
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public double SizeMB { get; set; }
        public bool Exists { get; set; } = true;

        // Metadados do deploy
        public string? Repository { get; set; }
        public string? Branch { get; set; }
        public string[]? BuildCommand { get; set; }
        public DateTime? DeployedAt { get; set; }

        /// <summary>
        /// Nome do projeto pai quando o deploy está em subdiretório.
        /// Null se estiver no nível raiz.
        /// Ex: "app2/api" → ParentProject = "app2", "app1" → ParentProject = null
        /// </summary>
        public string? ParentProject { get; set; }
    }
}
