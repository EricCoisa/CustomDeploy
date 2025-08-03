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
                var directories = Directory.GetDirectories(_publicationsPath);

                foreach (var directory in directories)
                {
                    try
                    {
                        var directoryInfo = new DirectoryInfo(directory);
                        var fullPath = directoryInfo.FullName;
                        var sizeInBytes = await CalculateDirectorySizeAsync(directory);
                        var sizeInMB = Math.Round(sizeInBytes / (1024.0 * 1024.0), 2);

                        // Buscar metadados no dicionário
                        metadataDict.TryGetValue(fullPath, out var metadata);

                        // Se não há metadados para um diretório existente, criar automaticamente
                        if (metadata == null)
                        {
                            _logger.LogInformation("Diretório encontrado sem metadados, criando automaticamente: {Path}", fullPath);
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

                        var publication = new PublicationInfo
                        {
                            Name = directoryInfo.Name,
                            FullPath = fullPath,
                            LastModified = directoryInfo.LastWriteTime,
                            SizeMB = sizeInMB,
                            Exists = metadata?.Exists ?? true, // Se não há metadados, assume que existe (pasta física existe)
                            Repository = metadata?.Repository,
                            Branch = metadata?.Branch,
                            BuildCommand = metadata?.BuildCommand,
                            DeployedAt = metadata?.DeployedAt
                        };

                        publications.Add(publication);
                        _logger.LogDebug("Publicação encontrada: {Name} - {SizeMB} MB - Exists: {Exists}", 
                            publication.Name, publication.SizeMB, publication.Exists);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar diretório: {Directory}", directory);
                    }
                }

                // Incluir deploys que não existem mais fisicamente
                foreach (var metadata in allDeployMetadata.Where(m => !m.Exists))
                {
                    var deployName = metadata.Name;
                    
                    // Verificar se já foi adicionado (caso o diretório ainda exista)
                    if (!publications.Any(p => string.Equals(p.Name, deployName, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("Deploy registrado mas diretório não existe: {TargetPath}", metadata.TargetPath);
                        
                        var offlinePublication = new PublicationInfo
                        {
                            Name = $"{deployName} (Removido)",
                            FullPath = metadata.TargetPath,
                            LastModified = metadata.DeployedAt,
                            SizeMB = 0,
                            Exists = false, // Explicitamente marcado como não existente
                            Repository = metadata.Repository,
                            Branch = metadata.Branch,
                            BuildCommand = metadata.BuildCommand,
                            DeployedAt = metadata.DeployedAt
                        };
                        
                        publications.Add(offlinePublication);
                    }
                }

                // Ordenar por data de modificação (mais recente primeiro)
                publications = publications.OrderByDescending(p => p.LastModified).ToList();

                _logger.LogInformation("Encontradas {Count} publicações ({ActiveCount} ativas, {InactiveCount} removidas)", 
                    publications.Count,
                    publications.Count(p => !p.Name.Contains("(Removido)")),
                    publications.Count(p => p.Name.Contains("(Removido)")));
                    
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
    }
}
