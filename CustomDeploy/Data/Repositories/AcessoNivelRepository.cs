using Microsoft.EntityFrameworkCore;
using CustomDeploy.Models.Entities;
using CustomDeploy.Data;

namespace CustomDeploy.Data.Repositories
{
    public class AcessoNivelRepository : Repository<AcessoNivel>, IAcessoNivelRepository
    {
        public AcessoNivelRepository(CustomDeployDbContext context) : base(context)
        {
        }

        public async Task<AcessoNivel?> GetByNomeAsync(string nome)
        {
            return await _dbSet
                .FirstOrDefaultAsync(an => an.Nome.ToLower() == nome.ToLower());
        }

        public async Task<bool> NomeExistsAsync(string nome)
        {
            return await _dbSet
                .AnyAsync(an => an.Nome.ToLower() == nome.ToLower());
        }
    }
}
