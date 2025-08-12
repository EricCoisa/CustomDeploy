using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Repositories
{
    public interface IAcessoNivelRepository : IRepository<AcessoNivel>
    {
        Task<AcessoNivel?> GetByNomeAsync(string nome);
        Task<bool> NomeExistsAsync(string nome);
    }
}
