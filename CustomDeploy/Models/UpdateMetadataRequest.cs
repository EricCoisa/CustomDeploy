namespace CustomDeploy.Models
{
    public class UpdateMetadataRequest
    {
        /// <summary>
        /// Novo repository URL (opcional)
        /// </summary>
        public string? Repository { get; set; }

        /// <summary>
        /// Nova branch (opcional)
        /// </summary>
        public string? Branch { get; set; }

        /// <summary>
        /// Novo comando de build (opcional)
        /// </summary>
        public string? BuildCommand { get; set; }
    }
}
