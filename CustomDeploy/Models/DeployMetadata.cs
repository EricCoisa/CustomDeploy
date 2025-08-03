namespace CustomDeploy.Models
{
    public class DeployMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string BuildCommand { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public DateTime DeployedAt { get; set; }
        public bool Exists { get; set; } = true;
    }
}
