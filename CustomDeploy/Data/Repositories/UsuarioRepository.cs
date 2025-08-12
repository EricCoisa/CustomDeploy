using Microsoft.EntityFrameworkCore;
using CustomDeploy.Models.Entities;
using CustomDeploy.Data;

namespace CustomDeploy.Data.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(CustomDeployDbContext context) : base(context)
        {
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<Usuario?> GetByEmailWithAcessoAsync(string email)
        {
            return await _dbSet
                .Include(u => u.UsuarioAcesso)
                .ThenInclude(ua => ua.AcessoNivel)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosAtivosAsync()
        {
            return await _dbSet
                .Include(u => u.UsuarioAcesso)
                .ThenInclude(ua => ua.AcessoNivel)
                .Where(u => u.Ativo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosByAcessoNivelAsync(int acessoNivelId)
        {
            return await _dbSet
                .Include(u => u.UsuarioAcesso)
                .ThenInclude(ua => ua.AcessoNivel)
                .Where(u => u.UsuarioAcesso.AcessoNivelId == acessoNivelId)
                .ToListAsync();
        }

        public override async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.UsuarioAcesso)
                .ThenInclude(ua => ua.AcessoNivel)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public override async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.UsuarioAcesso)
                .ThenInclude(ua => ua.AcessoNivel)
                .ToListAsync();
        }
    }
}
