using CustomDeploy.Models;

namespace CustomDeploy.Services
{
    /// <summary>
    /// Interface para serviços de gerenciamento do sistema de arquivos
    /// </summary>
    public interface IFileManagerService
    {
        /// <summary>
        /// Obtém o conteúdo de um diretório
        /// </summary>
        /// <param name="path">Caminho do diretório (opcional, padrão é C:\)</param>
        /// <param name="includeHidden">Incluir arquivos ocultos</param>
        /// <param name="fileExtensionFilter">Filtro por extensão</param>
        /// <param name="sortBy">Campo para ordenação</param>
        /// <param name="ascending">Ordem crescente</param>
        /// <returns>Resposta com o conteúdo do diretório</returns>
        Task<FileSystemResponse> GetDirectoryContentsAsync(
            string? path = null, 
            bool includeHidden = false,
            string? fileExtensionFilter = null,
            string sortBy = "name",
            bool ascending = true);

        /// <summary>
        /// Verifica se um caminho é válido e acessível
        /// </summary>
        /// <param name="path">Caminho para verificar</param>
        /// <returns>True se válido e acessível</returns>
        bool IsPathValidAndAccessible(string path);

        /// <summary>
        /// Verifica se um caminho está na lista de bloqueio (restrito)
        /// </summary>
        /// <param name="path">Caminho para verificar</param>
        /// <returns>True se estiver bloqueado</returns>
        bool IsPathBlocked(string path);

        /// <summary>
        /// Obtém informações de um item específico do sistema de arquivos
        /// </summary>
        /// <param name="path">Caminho do item</param>
        /// <returns>Informações do item ou null se não encontrado</returns>
        Task<FileSystemItem?> GetItemInfoAsync(string path);

        /// <summary>
        /// Obtém as unidades de disco disponíveis
        /// </summary>
        /// <returns>Lista de unidades de disco</returns>
        List<FileSystemItem> GetAvailableDrives();

        /// <summary>
        /// Cria uma nova pasta
        /// </summary>
        /// <param name="path">Caminho completo da nova pasta</param>
        /// <returns>True se criada com sucesso</returns>
        Task<bool> CreateDirectoryAsync(string path);

        /// <summary>
        /// Renomeia uma pasta
        /// </summary>
        /// <param name="oldPath">Caminho atual da pasta</param>
        /// <param name="newName">Novo nome da pasta</param>
        /// <returns>True se renomeada com sucesso</returns>
        Task<bool> RenameDirectoryAsync(string oldPath, string newName);

        /// <summary>
        /// Deleta uma pasta
        /// </summary>
        /// <param name="path">Caminho da pasta a ser deletada</param>
        /// <param name="recursive">Se deve deletar recursivamente (incluindo conteúdo)</param>
        /// <returns>True se deletada com sucesso</returns>
        Task<bool> DeleteDirectoryAsync(string path, bool recursive = false);
    }
}
