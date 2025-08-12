using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomDeploy.Models.Entities
{
    [Table("DeployComandos")]
    public class DeployComando
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int DeployId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Comando { get; set; } = string.Empty;

        [Required]
        public int Ordem { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } = "Pendente";

        [MaxLength(2000)]
        public string? Mensagem { get; set; }

        public DateTime? ExecutadoEm { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(DeployId))]
        public virtual Deploy Deploy { get; set; } = null!;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime? AtualizadoEm { get; set; }
    }
}
