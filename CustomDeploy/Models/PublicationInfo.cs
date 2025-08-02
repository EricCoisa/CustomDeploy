namespace CustomDeploy.Models
{
    public class PublicationInfo
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public double SizeMB { get; set; }

        // Metadados do deploy
        public string? Repository { get; set; }
        public string? Branch { get; set; }
        public string? BuildCommand { get; set; }
        public DateTime? DeployedAt { get; set; }
    }
}
