using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Repositories
{
    public interface IDeployRepository : IRepository<Deploy>
    {
        Task<IEnumerable<Deploy>> GetDeploysBySiteNameAsync(string siteName);
        Task<IEnumerable<Deploy>> GetDeploysByUsuarioAsync(int usuarioId);
        Task<IEnumerable<Deploy>> GetDeploysByStatusAsync(string status);
        Task<IEnumerable<Deploy>> GetDeploysByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Deploy?> GetDeployWithComandosAsync(int deployId);
        Task<Deploy?> GetDeployWithHistoricoAsync(int deployId);
        Task<Deploy?> GetDeployCompleteAsync(int deployId);
        Task<IEnumerable<Deploy>> GetRecentDeploysAsync(int count = 10);
    }
}
