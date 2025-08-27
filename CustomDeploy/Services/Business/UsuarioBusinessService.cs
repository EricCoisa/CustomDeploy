using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using CustomDeploy.Models.Entities;
using CustomDeploy.Data.Repositories;
using CustomDeploy.Utils;

namespace CustomDeploy.Services.Business
{
    public class UsuarioBusinessService : IUsuarioBusinessService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAcessoNivelRepository _acessoNivelRepository;
        private readonly ILogger<UsuarioBusinessService> _logger;

        public UsuarioBusinessService(
            IUsuarioRepository usuarioRepository,
            IAcessoNivelRepository acessoNivelRepository,
            ILogger<UsuarioBusinessService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _acessoNivelRepository = acessoNivelRepository;
            _logger = logger;
        }

        public async Task<Usuario?> AutenticarAsync(string email, string senha)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByEmailWithAcessoAsync(email);
                
                if (usuario == null || !usuario.Ativo)
                {
                    _logger.LogWarning("Tentativa de login com email inválido ou usuário inativo: {Email}", email);
                    return null;
                }

                if (!VerificarSenha(senha, usuario.Senha))
                {
                    _logger.LogWarning("Tentativa de login com senha incorreta para email: {Email}", email);
                    return null;
                }

                _logger.LogInformation("Login realizado com sucesso para o usuário: {Email}", email);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao autenticar usuário: {Email}", email);
                return null;
            }
        }

        public async Task<Usuario> CriarUsuarioAsync(string nome, string email, string senha, int acessoNivelId)
        {
            try
            {
                // Verificar se email já existe
                if (await EmailExisteAsync(email))
                {
                    throw new InvalidOperationException($"Email {email} já está em uso.");
                }

                // Verificar se o nível de acesso existe
                var acessoNivel = await _acessoNivelRepository.GetByIdAsync(acessoNivelId);
                if (acessoNivel == null)
                {
                    throw new InvalidOperationException($"Nível de acesso {acessoNivelId} não encontrado.");
                }

                // Criar UsuarioAcesso primeiro
                var usuarioAcesso = new UsuarioAcesso
                {
                    AcessoNivelId = acessoNivelId
                };

                // Criar o usuário
                var usuario = new Usuario
                {
                    Nome = nome,
                    Email = email,
                    Senha = GerarHashSenha(senha),
                    Ativo = true,
                    UsuarioAcesso = usuarioAcesso
                };

                // Definir a relação bidirecional
                usuarioAcesso.Usuario = usuario;

                await _usuarioRepository.AddAsync(usuario);
                await _usuarioRepository.SaveChangesAsync();

                _logger.LogInformation("Usuário criado com sucesso: {Email}", email);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário: {Email}", email);
                throw;
            }
        }

        public async Task<Usuario?> ObterUsuarioPorIdAsync(int id)
        {
            return await _usuarioRepository.GetByIdAsync(id);
        }

        public async Task<Usuario?> ObterUsuarioPorEmailAsync(string email)
        {
            return await _usuarioRepository.GetByEmailWithAcessoAsync(email);
        }

        public async Task<IEnumerable<Usuario>> ObterTodosUsuariosAsync()
        {
            return await _usuarioRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Usuario>> ObterUsuariosAtivosAsync()
        {
            return await _usuarioRepository.GetUsuariosAtivosAsync();
        }

        public async Task<bool> AtualizarUsuarioAsync(int id, string? nome = null, string? email = null, string? senha = null, bool? ativo = null)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                {
                    return false;
                }

                // Verificar se o novo email já existe (se fornecido)
                if (!string.IsNullOrWhiteSpace(email) && email != usuario.Email)
                {
                    if (await EmailExisteAsync(email))
                    {
                        throw new InvalidOperationException($"Email {email} já está em uso.");
                    }
                    usuario.Email = email;
                }

                if (!string.IsNullOrWhiteSpace(nome))
                    usuario.Nome = nome;

                if (!string.IsNullOrWhiteSpace(senha))
                    usuario.Senha = GerarHashSenha(senha);

                if (ativo.HasValue)
                    usuario.Ativo = ativo.Value;

                await _usuarioRepository.UpdateAsync(usuario);
                await _usuarioRepository.SaveChangesAsync();

                _logger.LogInformation("Usuário atualizado com sucesso: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExcluirUsuarioAsync(int id)
        {
            try
            {
                var result = await _usuarioRepository.DeleteAsync(id);
                if (result)
                {
                    await _usuarioRepository.SaveChangesAsync();
                    _logger.LogInformation("Usuário excluído com sucesso: {Id}", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário: {Id}", id);
                throw;
            }
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            return await _usuarioRepository.EmailExistsAsync(email);
        }

        public async Task<bool> VerificarPermissaoAsync(int usuarioId, string[] permissoesRequeridas)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null || !usuario.Ativo)
                return false;

            var nomeAcesso = usuario.UsuarioAcesso.AcessoNivel.Nome;

            // Administrador tem todas as permissões
            if (nomeAcesso == "Administrador")
                return true;

            // Aqui você pode implementar lógica específica de permissões
            // Por enquanto, operador tem permissões limitadas
            if (nomeAcesso == "Operador")
            {
                var permissoesOperador = new[] { "deploy:read", "deploy:create" };
                return permissoesRequeridas.All(p => permissoesOperador.Contains(p));
            }

            return false;
        }

        public async Task<bool> IsAdministradorAsync(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            return usuario?.UsuarioAcesso.AcessoNivel.Nome == "Administrador";
        }

        public string GerarHashSenha(string senha)
        {
            return AuthUtils.GerarHashSenha(senha);
        }

        public bool VerificarSenha(string senha, string hash)
        {
            return AuthUtils.VerificarSenha(senha, hash);
        }
    }
}
