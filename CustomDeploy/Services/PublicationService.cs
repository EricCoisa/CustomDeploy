using CustomDeploy.Models;
using System.Text.Json;

namespace CustomDeploy.Services
{
    public class PublicationService
    {
        private readonly ILogger<PublicationService> _logger;
        private readonly string _publicationsPath;

        public PublicationService(ILogger<PublicationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _publicationsPath = configuration.GetValue<string>("DeploySettings:PublicationsPath") 
                ?? "C:\\inetpub\\wwwroot";
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

                var publications = new List<PublicationInfo>();
                var directories = Directory.GetDirectories(_publicationsPath);

                foreach (var directory in directories)
                {
                    try
                    {
                        var directoryInfo = new DirectoryInfo(directory);
                        var sizeInBytes = await CalculateDirectorySizeAsync(directory);
                        var sizeInMB = Math.Round(sizeInBytes / (1024.0 * 1024.0), 2);

                        // Carregar metadados do deploy se existir
                        var metadata = await LoadDeployMetadataAsync(directory);

                        var publication = new PublicationInfo
                        {
                            Name = directoryInfo.Name,
                            FullPath = directoryInfo.FullName,
                            LastModified = directoryInfo.LastWriteTime,
                            SizeMB = sizeInMB,
                            Repository = metadata?.Repository,
                            Branch = metadata?.Branch,
                            BuildCommand = metadata?.BuildCommand,
                            DeployedAt = metadata?.DeployedAt
                        };

                        publications.Add(publication);
                        _logger.LogDebug("Publicação encontrada: {Name} - {SizeMB} MB", publication.Name, publication.SizeMB);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar diretório: {Directory}", directory);
                    }
                }

                // Ordenar por data de modificação (mais recente primeiro)
                publications = publications.OrderByDescending(p => p.LastModified).ToList();

                _logger.LogInformation("Encontradas {Count} publicações", publications.Count);
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
                var publications = await GetPublicationsAsync();
                return publications.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar publicação: {Name}", name);
                throw;
            }
        }

        private async Task<DeployMetadata?> LoadDeployMetadataAsync(string directoryPath)
        {
            try
            {
                var metadataPath = Path.Combine(directoryPath, "deploy.json");
                
                if (!File.Exists(metadataPath))
                {
                    _logger.LogDebug("Arquivo deploy.json não encontrado em: {DirectoryPath}", directoryPath);
                    return null;
                }

                var jsonContent = await File.ReadAllTextAsync(metadataPath);
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var metadata = JsonSerializer.Deserialize<DeployMetadata>(jsonContent, jsonOptions);
                _logger.LogDebug("Metadados carregados para: {DirectoryPath}", directoryPath);
                
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao carregar metadados de: {DirectoryPath}", directoryPath);
                return null;
            }
        }
    }
}
