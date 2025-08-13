using Microsoft.EntityFrameworkCore;
using CustomDeploy.Models.Entities;
using CustomDeploy.Data;

namespace CustomDeploy.Data.Repositories
{
    public class DeployRepository : Repository<Deploy>, IDeployRepository
    {
        public DeployRepository(CustomDeployDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Deploy>> GetDeploysBySiteNameAsync(string siteName)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .Where(d => d.SiteName.ToLower() == siteName.ToLower())
                .OrderByDescending(d => d.Data)
                .ToListAsync();
        }

        public async Task<IEnumerable<Deploy>> GetDeploysByUsuarioAsync(int usuarioId)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .Where(d => d.UsuarioId == usuarioId)
                .OrderByDescending(d => d.Data)
                .ToListAsync();
        }

        public async Task<IEnumerable<Deploy>> GetDeploysByStatusAsync(string status)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .Where(d => d.Status.ToLower() == status.ToLower())
                .OrderByDescending(d => d.Data)
                .ToListAsync();
        }

        public async Task<IEnumerable<Deploy>> GetDeploysByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .Where(d => d.Data >= startDate && d.Data <= endDate)
                .OrderByDescending(d => d.Data)
                .ToListAsync();
        }

        public async Task<Deploy?> GetDeployWithComandosAsync(int deployId)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .FirstOrDefaultAsync(d => d.Id == deployId);
        }

        public async Task<Deploy?> GetDeployWithHistoricoAsync(int deployId)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .Include(d => d.DeployHistoricos.OrderByDescending(dh => dh.Data))
                .FirstOrDefaultAsync(d => d.Id == deployId);
        }

        public async Task<Deploy?> GetDeployCompleteAsync(int deployId)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .ThenInclude(u => u.UsuarioAcesso)
                .ThenInclude(ua => ua.AcessoNivel)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .Include(d => d.DeployHistoricos.OrderByDescending(dh => dh.Data))
                .FirstOrDefaultAsync(d => d.Id == deployId);
        }

        public async Task<IEnumerable<Deploy>> GetRecentDeploysAsync(int count = 10)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .OrderByDescending(d => d.Data)
                .Take(count)
                .ToListAsync();
        }

        public override async Task<Deploy?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public override async Task<IEnumerable<Deploy>> GetAllAsync()
        {
            return await _dbSet
                .Include(d => d.Usuario)
                .Include(d => d.DeployComandos.OrderBy(dc => dc.Ordem))
                .OrderByDescending(d => d.Data)
                .ToListAsync();
        }
    }
}
