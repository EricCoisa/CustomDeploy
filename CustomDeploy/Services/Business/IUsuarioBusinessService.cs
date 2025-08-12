using CustomDeploy.Models.Entities;
using CustomDeploy.Models;

namespace CustomDeploy.Services.Business
{
    public interface IUsuarioBusinessService
    {
        Task<Usuario?> AutenticarAsync(string email, string senha);
        Task<Usuario> CriarUsuarioAsync(string nome, string email, string senha, int acessoNivelId);
        Task<Usuario?> ObterUsuarioPorIdAsync(int id);
        Task<Usuario?> ObterUsuarioPorEmailAsync(string email);
        Task<IEnumerable<Usuario>> ObterTodosUsuariosAsync();
        Task<IEnumerable<Usuario>> ObterUsuariosAtivosAsync();
        Task<bool> AtualizarUsuarioAsync(int id, string? nome = null, string? email = null, string? senha = null, bool? ativo = null);
        Task<bool> ExcluirUsuarioAsync(int id);
        Task<bool> EmailExisteAsync(string email);
        Task<bool> VerificarPermissaoAsync(int usuarioId, string[] permissoesRequeridas);
        Task<bool> IsAdministradorAsync(int usuarioId);
        string GerarHashSenha(string senha);
        bool VerificarSenha(string senha, string hash);
    }
}
