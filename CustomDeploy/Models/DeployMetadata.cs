namespace CustomDeploy.Models
{
    public class DeployMetadata
    {
        public string Repository { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string BuildCommand { get; set; } = string.Empty;
        public DateTime DeployedAt { get; set; }
    }
}
