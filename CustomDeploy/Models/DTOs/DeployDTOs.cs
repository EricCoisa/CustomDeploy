using System.ComponentModel.DataAnnotations;

namespace CustomDeploy.Models.DTOs
{
    public class CriarDeployRequest
    {
        [Required(ErrorMessage = "Nome do site é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome do site deve ter no máximo 200 caracteres")]
        public string SiteName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Nome da aplicação deve ter no máximo 200 caracteres")]
        public string? ApplicationName { get; set; }

        [Required(ErrorMessage = "URL do repositório é obrigatória")]
        [StringLength(500, ErrorMessage = "URL do repositório deve ter no máximo 500 caracteres")]
        public string RepoUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch é obrigatória")]
        [StringLength(100, ErrorMessage = "Branch deve ter no máximo 100 caracteres")]
        public string Branch { get; set; } = "main";

        [Required(ErrorMessage = "Comandos são obrigatórios")]
        [MinLength(1, ErrorMessage = "Deve haver pelo menos um comando")]
        public BuildCommand[] BuildCommand { get; set; } = Array.Empty<BuildCommand>();


        [Required(ErrorMessage = "Saída do build é obrigatória")]
        [StringLength(200, ErrorMessage = "Saída do build deve ter no máximo 200 caracteres")]
        public string BuildOutput { get; set; } = "dist";

        [StringLength(100, ErrorMessage = "Plataforma deve ter no máximo 100 caracteres")]
        public string? Plataforma { get; set; }
    }

    public class CriarDeployCompletoRequest
    {
        [Required(ErrorMessage = "URL do repositório é obrigatória")]
        [StringLength(500, ErrorMessage = "URL do repositório deve ter no máximo 500 caracteres")]
        public string RepoUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch é obrigatória")]
        [StringLength(100, ErrorMessage = "Branch deve ter no máximo 100 caracteres")]
        public string Branch { get; set; } = string.Empty;

        [Required(ErrorMessage = "Comandos de build são obrigatórios")]
        [MinLength(1, ErrorMessage = "Deve haver pelo menos um comando de build")]
        public BuildCommand[] BuildCommand { get; set; } = Array.Empty<BuildCommand>();

        [Required(ErrorMessage = "Saída do build é obrigatória")]
        [StringLength(200, ErrorMessage = "Saída do build deve ter no máximo 200 caracteres")]
        public string BuildOutput { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome do site IIS é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome do site IIS deve ter no máximo 200 caracteres")]
        public string IisSiteName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Caminho de destino deve ter no máximo 200 caracteres")]
        public string? TargetPath { get; set; }

        [StringLength(200, ErrorMessage = "Caminho da aplicação deve ter no máximo 200 caracteres")]
        public string? ApplicationPath { get; set; }

        [StringLength(100, ErrorMessage = "Plataforma deve ter no máximo 100 caracteres")]
        public string? Plataforma { get; set; }
    }

    public class AtualizarStatusDeployRequest
    {
        [Required(ErrorMessage = "Status é obrigatório")]
        [StringLength(50, ErrorMessage = "Status deve ter no máximo 50 caracteres")]
        public string Status { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mensagem deve ter no máximo 2000 caracteres")]
        public string? Mensagem { get; set; }
    }

    public class DeployResponse
    {
        public int Id { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string? ApplicationName { get; set; }
        public DateTime Data { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Mensagem { get; set; }
        public string? Plataforma { get; set; }
        public UsuarioResponse Usuario { get; set; } = new();
        public List<DeployComandoResponse> Comandos { get; set; } = new();
        public List<DeployHistoricoResponse> Historico { get; set; } = new();
        
        // Dados do deploy no IIS (se aplicável)
        public string? RepoUrl { get; set; }
        public string? Branch { get; set; }
        public BuildCommand[]? BuildCommands { get; set; }
        public string? BuildOutput { get; set; }
        public string? TargetPath { get; set; }
        public string? ApplicationPath { get; set; }
        public object? DeployDetails { get; set; }
    }

    public class DeployComandoResponse
    {
        public int Id { get; set; }
        public string Comando { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public DateTime CriadoEm { get; set; }
        public string? Status { get; set; }
        public string? Mensagem { get; set; }
        public DateTime? ExecutadoEm { get; set; }
        public string? TerminalId { get; set; } // Adicionado o campo TerminalId
    }

    public class DeployHistoricoResponse
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Mensagem { get; set; }
    }

    public class DeployResumoResponse
    {
        public int Id { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string? ApplicationName { get; set; }
        public DateTime Data { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UsuarioNome { get; set; } = string.Empty;
        public string? Plataforma { get; set; }
    }

    public class BuildCommand
    {
        public string Comando { get; set; } = string.Empty;
        public string? TerminalId { get; set; }
        public int Ordem { get; set; }
        public string? Status { get; set; } = string.Empty;
    }
}
