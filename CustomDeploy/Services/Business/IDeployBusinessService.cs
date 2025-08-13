using CustomDeploy.Models.Entities;
using CustomDeploy.Models;
using CustomDeploy.Models.DTOs;

namespace CustomDeploy.Services.Business
{
    public interface IDeployBusinessService
    {
        Task<Deploy> CriarDeployAsync(string siteName, string? applicationName, int usuarioId,
            BuildCommand[] BuildCommands, string repoUrl, string branch = "main", string buildOutput = "dist", string? plataforma = null);
        Task<Deploy> ExecuteDeployCompletoAsync(DeployRequest deployRequest, int usuarioId);
        Task<Deploy?> ObterDeployPorIdAsync(int id);
        Task<Deploy?> ObterDeployCompletoAsync(int id);
        Task<IEnumerable<Deploy>> ObterTodosDeploysAsync();
        Task<IEnumerable<Deploy>> ObterDeploysPorSiteAsync(string siteName);
        Task<IEnumerable<Deploy>> ObterDeploysPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Deploy>> ObterDeploysRecentesAsync(int quantidade = 10);
        Task<bool> AtualizarStatusDeployAsync(int deployId, string status, string? mensagem = null);
        Task<bool> AdicionarComandoAsync(int deployId, string comando, int ordem, string terminalId = "1");
        Task<bool> AdicionarHistoricoAsync(int deployId, string status, string? mensagem = null);
        Task<IEnumerable<DeployHistorico>> ObterHistoricoDeployAsync(int deployId);
        Task<IEnumerable<DeployComando>> ObterComandosDeployAsync(int deployId);
        Task<bool> ExcluirDeployAsync(int id);
        Task<(bool Success, string Message)> ValidateRepositoryAsync(string repoUrl);
        Task<(bool Success, string Message)> ValidateBranchAsync(string repoUrl, string branch);
        // Task<(bool Success, string Message)> CloneRepositoryAsync(string repoUrl, string branch, string targetPath);
    }
}
