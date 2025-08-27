using System.Security.Cryptography;
using System.Text;

namespace CustomDeploy.Utils
{
    public static class AuthUtils
    {
        public static string GerarHashSenha(string senha)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha + "CustomDeploy_Salt"));
            return Convert.ToBase64String(hashedBytes);
        }

        public static bool VerificarSenha(string senha, string hash)
        {
            var senhaHash = GerarHashSenha(senha);
            return senhaHash == hash;
        }
    }
}
