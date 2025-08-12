using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomDeploy.Models.Entities
{
    [Table("Deploys")]
    public class Deploy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string SiteName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ApplicationName { get; set; }

        [Required]
        public DateTime Data { get; set; } = DateTime.UtcNow;

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Mensagem { get; set; }

        [MaxLength(100)]
        public string? Plataforma { get; set; }

        [MaxLength(200)]
        public string? RepoUrl { get; set; }

        [MaxLength(100)]
        public string? Branch { get; set; }

        [MaxLength(100)]
        public string? BuildOutput { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = null!;

        public virtual ICollection<DeployComando> DeployComandos { get; set; } = new List<DeployComando>();
        public virtual ICollection<DeployHistorico> DeployHistoricos { get; set; } = new List<DeployHistorico>();

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime? AtualizadoEm { get; set; }
    }
}
