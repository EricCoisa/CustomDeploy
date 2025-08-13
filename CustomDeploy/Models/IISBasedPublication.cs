using CustomDeploy.Models.Entities;

namespace CustomDeploy.Models
{
    /// <summary>
    /// Representa uma publicação baseada no IIS como fonte de verdade
    /// </summary>
    public class IISBasedPublication
    {
        /// <summary>
        /// Nome do site IIS
        /// </summary>
        public string IisSite { get; set; } = string.Empty;

        /// <summary>
        /// Caminho físico do site IIS
        /// </summary>
        public string IisPath { get; set; } = string.Empty;

        /// <summary>
        /// Nome da subaplicação (null se for o site raiz)
        /// </summary>
        public string? SubApplication { get; set; }

        /// <summary>
        /// Caminho físico completo (site + subaplicação se houver)
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// URL do repositório (do arquivo de metadados)
        /// </summary>
        public string? RepoUrl { get; set; }

        /// <summary>
        /// Branch do repositório (do arquivo de metadados)
        /// </summary>
        public string? Branch { get; set; }

        /// <summary>
        /// Comando de build (do arquivo de metadados)
        /// </summary>
        public virtual ICollection<DeployComando> BuildCommand { get; set; } = new List<DeployComando>();

        /// <summary>
        /// Diretório de saída do build (do arquivo de metadados)
        /// </summary>
        public string? BuildOutput { get; set; }

        /// <summary>
        /// Data do último deploy (do arquivo de metadados)
        /// </summary>
        public DateTime? DeployedAt { get; set; }

        /// <summary>
        /// Se existe no IIS (sempre true para publicações baseadas no IIS)
        /// </summary>
        public bool Exists { get; set; } = true;

        /// <summary>
        /// Estado do site no IIS
        /// </summary>
        public int? IisSiteState { get; set; }

        /// <summary>
        /// ID do site no IIS
        /// </summary>
        public int? IisSiteId { get; set; }

        /// <summary>
        /// Application Pool da aplicação (se for subaplicação)
        /// </summary>
        public string? ApplicationPool { get; set; }

        /// <summary>
        /// Protocolos habilitados (se for subaplicação)
        /// </summary>
        public string? EnabledProtocols { get; set; }

        /// <summary>
        /// Tamanho em MB (calculado do diretório físico)
        /// </summary>
        public double SizeMB { get; set; }

        /// <summary>
        /// Última modificação do diretório físico
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// Nome único para identificação (site/aplicação ou apenas site)
        /// </summary>
        public string Name => string.IsNullOrWhiteSpace(SubApplication) 
            ? IisSite 
            : $"{IisSite}/{SubApplication}";

        /// <summary>
        /// Se tem metadados associados
        /// </summary>
        public bool HasMetadata => !string.IsNullOrWhiteSpace(RepoUrl);
    }
}
