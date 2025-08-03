namespace CustomDeploy.Models
{
    /// <summary>
    /// Request para criação de site no IIS
    /// </summary>
    public class CreateSiteRequest
    {
        /// <summary>
        /// Nome do site no IIS
        /// </summary>
        public string SiteName { get; set; } = string.Empty;

        /// <summary>
        /// Porta do site (ex: 80, 8080, 443)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Caminho físico do site (ex: C:\inetpub\wwwroot\meusite)
        /// </summary>
        public string PhysicalPath { get; set; } = string.Empty;

        /// <summary>
        /// Nome do Application Pool (ex: MeuSiteAppPool)
        /// </summary>
        public string AppPool { get; set; } = string.Empty;
    }
}
