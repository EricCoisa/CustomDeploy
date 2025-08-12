using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByEmailWithAcessoAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<Usuario>> GetUsuariosAtivosAsync();
        Task<IEnumerable<Usuario>> GetUsuariosByAcessoNivelAsync(int acessoNivelId);
    }
}
