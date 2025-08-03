using CustomDeploy.Models;
using System.Text.Json;

namespace CustomDeploy.Services
{
    public class PublicationService
    {
        private readonly ILogger<PublicationService> _logger;
        private readonly string _publicationsPath;
        private readonly DeployService _deployService;

        public PublicationService(ILogger<PublicationService> logger, IConfiguration configuration, DeployService deployService)
        {
            _logger = logger;
            _publicationsPath = configuration.GetValue<string>("DeploySettings:PublicationsPath") 
                ?? "C:\\inetpub\\wwwroot";
            _deployService = deployService;
        }

        public async Task<List<PublicationInfo>> GetPublicationsAsync()
        {
            try
            {
                _logger.LogInformation("Listando publicações em: {PublicationsPath}", _publicationsPath);

                if (!Directory.Exists(_publicationsPath))
                {
                    _logger.LogWarning("Diretório de publicações não encontrado: {PublicationsPath}", _publicationsPath);
                    return new List<PublicationInfo>();
                }

                // Carregar todos os metadados do arquivo centralizado com verificação de existência
                var allDeployMetadata = _deployService.GetAllDeployMetadataWithExistsCheck();
                var metadataDict = allDeployMetadata.ToDictionary(
                    m => Path.GetFullPath(m.TargetPath), 
                    m => m, 
                    StringComparer.OrdinalIgnoreCase);

                var publications = new List<PublicationInfo>();

                // 1. Buscar apenas diretórios no nível raiz e adicionar aos metadados se necessário
                var rootDirectories = Directory.GetDirectories(_publicationsPath);
                foreach (var directory in rootDirectories)
                {
                    try
                    {
                        var directoryInfo = new DirectoryInfo(directory);
                        var fullPath = directoryInfo.FullName;

                        _logger.LogDebug("Processando diretório raiz: {Directory}", fullPath);

                        // Buscar metadados no dicionário
                        metadataDict.TryGetValue(fullPath, out var metadata);

                        // Se não há metadados para um diretório existente, criar automaticamente
                        if (metadata == null)
                        {
                            _logger.LogInformation("Diretório raiz encontrado sem metadados, criando automaticamente: {Path}", fullPath);
                            var createResult = _deployService.CreateMetadataForExistingDirectory(fullPath);
                            
                            if (createResult.Success)
                            {
                                // Recarregar metadados após criação
                                var updatedMetadata = _deployService.GetAllDeployMetadataWithExistsCheck();
                                var updatedDict = updatedMetadata.ToDictionary(
                                    m => Path.GetFullPath(m.TargetPath), 
                                    m => m, 
                                    StringComparer.OrdinalIgnoreCase);
                                updatedDict.TryGetValue(fullPath, out metadata);
                                
                                _logger.LogInformation("Metadados criados automaticamente: {Message}", createResult.Message);
                            }
                            else
                            {
                                _logger.LogWarning("Falha ao criar metadados automáticos: {Message}", createResult.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar diretório raiz: {Directory}", directory);
                    }
                }

                // 2. Recarregar todos os metadados após possíveis criações automáticas
                allDeployMetadata = _deployService.GetAllDeployMetadataWithExistsCheck();

                // 3. Criar PublicationInfo para todos os deploys dos metadados
                foreach (var metadata in allDeployMetadata)
                {
                    try
                    {
                        var fullPath = Path.GetFullPath(metadata.TargetPath);
                        var directoryExists = Directory.Exists(fullPath);
                        
                        // Calcular tamanho se o diretório existir fisicamente
                        double sizeInMB = 0;
                        DateTime lastModified = metadata.DeployedAt;
                        
                        if (directoryExists)
                        {
                            var directoryInfo = new DirectoryInfo(fullPath);
                            var sizeInBytes = await CalculateDirectorySizeAsync(fullPath);
                            sizeInMB = Math.Round(sizeInBytes / (1024.0 * 1024.0), 2);
                            lastModified = directoryInfo.LastWriteTime;
                        }

                        var publication = new PublicationInfo
                        {
                            Name = metadata.Exists && directoryExists ? metadata.Name : $"{metadata.Name} (Removido)",
                            FullPath = fullPath,
                            LastModified = lastModified,
                            SizeMB = sizeInMB,
                            Exists = metadata.Exists && directoryExists,
                            Repository = metadata.Repository,
                            Branch = metadata.Branch,
                            BuildCommand = metadata.BuildCommand,
                            DeployedAt = metadata.DeployedAt,
                            ParentProject = ExtractParentProject(GetRelativePathFromFull(metadata.TargetPath))
                        };

                        publications.Add(publication);
                        
                        _logger.LogDebug("Publicação dos metadados: {Name} - {SizeMB} MB - Exists: {Exists} - TargetPath: {TargetPath}", 
                            publication.Name, publication.SizeMB, publication.Exists, metadata.TargetPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar metadata do deploy: {Name}", metadata.Name);
                    }
                }

                // Ordenar por data de modificação (mais recente primeiro)
                publications = publications.OrderByDescending(p => p.LastModified).ToList();

                _logger.LogInformation("Encontradas {Count} publicações ({ActiveCount} ativas, {InactiveCount} removidas)", 
                    publications.Count,
                    publications.Count(p => p.Exists),
                    publications.Count(p => !p.Exists));
                    
                return publications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar publicações");
                throw;
            }
        }

        private async Task<long> CalculateDirectorySizeAsync(string directoryPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var directory = new DirectoryInfo(directoryPath);
                    return CalculateDirectorySize(directory);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao calcular tamanho do diretório: {Directory}", directoryPath);
                    return 0;
                }
            });
        }

        private long CalculateDirectorySize(DirectoryInfo directory)
        {
            long size = 0;

            try
            {
                // Somar tamanho de todos os arquivos
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                {
                    try
                    {
                        size += file.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignorar arquivos sem permissão de acesso
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Erro ao acessar arquivo: {FileName}", file.FullName);
                    }
                }

                // Somar tamanho de subdiretórios recursivamente
                DirectoryInfo[] subdirectories = directory.GetDirectories();
                foreach (DirectoryInfo subdirectory in subdirectories)
                {
                    try
                    {
                        size += CalculateDirectorySize(subdirectory);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignorar diretórios sem permissão de acesso
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Erro ao acessar subdiretório: {DirectoryName}", subdirectory.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Erro ao calcular tamanho do diretório: {DirectoryName}", directory.FullName);
            }

            return size;
        }

        public async Task<PublicationInfo?> GetPublicationByNameAsync(string name)
        {
            try
            {
                // Primeiro, verificar se existe um diretório físico com esse nome
                var potentialPath = Path.Combine(_publicationsPath, name);
                var directoryExists = Directory.Exists(potentialPath);
                
                if (directoryExists)
                {
                    // Verificar se há metadados para este diretório
                    var allMetadata = _deployService.GetAllDeployMetadata();
                    var metadata = allMetadata.FirstOrDefault(m => 
                        string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(Path.GetFileName(m.TargetPath), name, StringComparison.OrdinalIgnoreCase));
                    
                    if (metadata == null)
                    {
                        _logger.LogInformation("Diretório '{Name}' encontrado sem metadados, criando automaticamente", name);
                        var createResult = _deployService.CreateMetadataForExistingDirectory(potentialPath);
                        
                        if (createResult.Success)
                        {
                            _logger.LogInformation("Metadados criados automaticamente para: {Name}", name);
                        }
                    }
                }
                
                // Agora usar o método padrão que já incluirá os metadados criados
                var publications = await GetPublicationsAsync();
                return publications.FirstOrDefault(p => 
                    p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Equals($"{name} (Removido)", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar publicação: {Name}", name);
                throw;
            }
        }

        /// <summary>
        /// Converte um caminho completo para um caminho relativo baseado no _publicationsPath.
        /// Ex: "C:\temp\wwwroot\carteira\api" -> "carteira/api"
        /// </summary>
        /// <param name="fullPath">Caminho completo</param>
        /// <returns>Caminho relativo ou o caminho original se não for possível converter</returns>
        private string GetRelativePathFromFull(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return string.Empty;
            }

            try
            {
                var normalizedFullPath = Path.GetFullPath(fullPath);
                var normalizedPublicationsPath = Path.GetFullPath(_publicationsPath);

                // Verificar se o caminho está dentro do diretório de publicações
                if (normalizedFullPath.StartsWith(normalizedPublicationsPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Extrair a parte relativa
                    var relativePath = normalizedFullPath.Substring(normalizedPublicationsPath.Length);
                    
                    // Remover separadores iniciais
                    relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    
                    // Normalizar separadores para /
                    relativePath = relativePath.Replace('\\', '/');
                    
                    return relativePath;
                }

                _logger.LogWarning("Caminho não está dentro do diretório de publicações: {FullPath}", fullPath);
                return fullPath; // Retorna o original se não conseguir converter
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao converter caminho completo para relativo: {FullPath}", fullPath);
                return fullPath; // Retorna o original em caso de erro
            }
        }

        /// <summary>
        /// Extrai o nome do projeto pai baseado na targetPath.
        /// Retorna null se estiver no nível raiz, ou o nome da pasta pai se estiver em subdiretório.
        /// </summary>
        /// <param name="targetPath">Caminho do deploy (ex: "app2/api", "app1")</param>
        /// <returns>Nome do projeto pai ou null</returns>
        private string? ExtractParentProject(string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                return null;
            }

            try
            {
                // Normalizar separadores de caminho para / para consistência
                var normalizedPath = targetPath.Replace('\\', '/');
                
                // Remover barras iniciais e finais
                normalizedPath = normalizedPath.Trim('/');
                
                // Se não há barras, está no nível raiz
                if (!normalizedPath.Contains('/'))
                {
                    return null;
                }
                
                // Extrair a primeira parte (projeto pai)
                var pathParts = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length > 1)
                {
                    return pathParts[0];
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao extrair projeto pai de: {TargetPath}", targetPath);
                return null;
            }
        }
    }
}
