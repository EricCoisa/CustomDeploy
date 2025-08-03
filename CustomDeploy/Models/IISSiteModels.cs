namespace CustomDeploy.Models
{
    /// <summary>
    /// Informações de um site IIS
    /// </summary>
    public class SiteInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public int State { get; set; }
        public string PhysicalPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Informações de uma aplicação IIS
    /// </summary>
    public class ApplicationInfo
    {
        public string Name { get; set; } = string.Empty;
        public string PhysicalPath { get; set; } = string.Empty;
        public string EnabledProtocols { get; set; } = string.Empty;
        public string ApplicationPool { get; set; } = string.Empty;
    }
}
