namespace CustomDeploy.Models
{
    /// <summary>
    /// Representa um item do sistema de arquivos (arquivo ou diretório)
    /// </summary>
    public class FileSystemItem
    {
        /// <summary>
        /// Nome do arquivo ou diretório
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Caminho completo do item
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do item (file ou directory)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o item é um diretório
        /// </summary>
        public bool IsDirectory => Type == "directory";

        /// <summary>
        /// Tamanho do arquivo em bytes (null para diretórios)
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// Data e hora da última modificação
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Indica se o item é acessível (permissões)
        /// </summary>
        public bool IsAccessible { get; set; } = true;

        /// <summary>
        /// Extensão do arquivo (apenas para arquivos)
        /// </summary>
        public string? Extension { get; set; }
    }

    /// <summary>
    /// Resposta da navegação do sistema de arquivos
    /// </summary>
    public class FileSystemResponse
    {
        /// <summary>
        /// Caminho atual sendo navegado
        /// </summary>
        public string CurrentPath { get; set; } = string.Empty;

        /// <summary>
        /// Caminho do diretório pai (null se for raiz)
        /// </summary>
        public string? ParentPath { get; set; }

        /// <summary>
        /// Lista de itens encontrados no diretório
        /// </summary>
        public List<FileSystemItem> Items { get; set; } = new List<FileSystemItem>();

        /// <summary>
        /// Total de itens encontrados
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total de diretórios
        /// </summary>
        public int TotalDirectories { get; set; }

        /// <summary>
        /// Total de arquivos
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Indica se o diretório é acessível
        /// </summary>
        public bool IsAccessible { get; set; } = true;

        /// <summary>
        /// Mensagem de erro, se houver
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp da consulta
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Request para navegação do sistema de arquivos
    /// </summary>
    public class FileSystemRequest
    {
        /// <summary>
        /// Caminho para navegar (opcional, padrão é C:\)
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Incluir arquivos ocultos (padrão: false)
        /// </summary>
        public bool IncludeHidden { get; set; } = false;

        /// <summary>
        /// Filtro por extensão de arquivo (opcional)
        /// </summary>
        public string? FileExtensionFilter { get; set; }

        /// <summary>
        /// Ordenação (name, size, date)
        /// </summary>
        public string SortBy { get; set; } = "name";

        /// <summary>
        /// Ordem crescente ou decrescente
        /// </summary>
        public bool Ascending { get; set; } = true;
    }

    /// <summary>
    /// Request para criação de diretório
    /// </summary>
    public class CreateDirectoryRequest
    {
        /// <summary>
        /// Caminho completo da nova pasta
        /// </summary>
        public string Path { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para renomeação de diretório
    /// </summary>
    public class RenameDirectoryRequest
    {
        /// <summary>
        /// Caminho atual da pasta
        /// </summary>
        public string OldPath { get; set; } = string.Empty;

        /// <summary>
        /// Novo nome da pasta
        /// </summary>
        public string NewName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para deleção de diretório
    /// </summary>
    public class DeleteDirectoryRequest
    {
        /// <summary>
        /// Caminho da pasta a ser deletada
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Se deve deletar recursivamente (incluindo conteúdo)
        /// </summary>
        public bool Recursive { get; set; } = false;
    }
}
