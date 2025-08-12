using CustomDeploy.Models;
using System.Text.Json;

namespace CustomDeploy.Services
{
    public class PublicationService
    {
        private readonly ILogger<PublicationService> _logger;
        private readonly DeployService _deployService;
        private readonly IISManagementService _iisManagementService;

        public PublicationService(ILogger<PublicationService> logger, IConfiguration configuration, DeployService deployService, IISManagementService iisManagementService)
        {
            _logger = logger;
            _deployService = deployService;
            _iisManagementService = iisManagementService;
        }

        /// <summary>
        /// Lista todas as publicações usando o IIS como fonte de verdade
        /// </summary>
        /// <returns>Lista de publicações baseadas no IIS</returns>
        public async Task<List<IISBasedPublication>> GetPublicationsAsync()
        {
            try
            {
                _logger.LogInformation("Listando publicações usando IIS como fonte de verdade");

                var publications = new List<IISBasedPublication>();

                // 1. Obter todos os sites do IIS
                var sitesResult = await _iisManagementService.GetAllSitesAsync();
                if (!sitesResult.Success)
                {
                    _logger.LogError("Erro ao obter sites do IIS: {Message}", sitesResult.Message);
                    return publications;
                }

                _logger.LogInformation("Sites encontrados no IIS: {Count}", sitesResult.Sites.Count);

                // 2. Processar cada site
                foreach (var siteObj in sitesResult.Sites)
                {
                    try
                    {
                        _logger.LogDebug("Processando site objeto: {SiteObj}", JsonSerializer.Serialize(siteObj));

                        _logger.LogDebug("Site objeto raw: {SiteObj}", JsonSerializer.Serialize(siteObj));
                        
                        var site = JsonSerializer.Deserialize<SiteInfo>(
                            JsonSerializer.Serialize(siteObj));

                        if (site == null) 
                        {
                            _logger.LogWarning("Site deserializado como null");
                            continue;
                        }

                        _logger.LogInformation("Site deserializado: Name='{Name}', ID={Id}, Path='{Path}'", 
                            site.Name ?? "NULL", site.Id, site.PhysicalPath ?? "NULL");

                        // 2.1. Adicionar o site raiz
                        var sitePublication = await CreatePublicationFromSite(site, null);
                        publications.Add(sitePublication);

                        // 2.2. Obter aplicações do site
                        var appsResult = await _iisManagementService.GetSiteApplicationsAsync(site.Name);
                        if (appsResult.Success)
                        {
                            var applications = appsResult.Data as List<object> ?? new List<object>();
                            foreach (var appObj in applications)
                            {
                                try
                                {
                                    var app = JsonSerializer.Deserialize<ApplicationInfo>(
                                        JsonSerializer.Serialize(appObj));

                                    if (app == null || string.IsNullOrWhiteSpace(app.Name)) continue;

                                    // Limpar o nome da aplicação (remover "/" inicial)
                                    var cleanAppName = app.Name.TrimStart('/');
                                    if (string.IsNullOrWhiteSpace(cleanAppName)) continue;

                                    var appPublication = await CreatePublicationFromSite(site, app);
                                    publications.Add(appPublication);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Erro ao processar aplicação do site {SiteName}", site.Name);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar site individual");
                    }
                }

                _logger.LogInformation("Encontradas {Count} publicações no IIS", publications.Count);
                return publications.OrderBy(p => p.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar publicações do IIS");
                return new List<IISBasedPublication>();
            }
        }

        /// <summary>
        /// Cria uma publicação baseada em informações do IIS
        /// </summary>
        private async Task<IISBasedPublication> CreatePublicationFromSite(
            SiteInfo site, 
            ApplicationInfo? app)
        {
            var publication = new IISBasedPublication
            {
                IisSite = site.Name,
                IisPath = site.PhysicalPath,
                IisSiteState = site.State,
                IisSiteId = site.Id,
                Exists = true // No IIS, então existe
            };

            // Definir aplicação e caminho completo
            if (app != null)
            {
                var cleanAppName = app.Name.TrimStart('/');
                publication.SubApplication = cleanAppName;
                publication.FullPath = app.PhysicalPath;
                publication.ApplicationPool = app.ApplicationPool;
                publication.EnabledProtocols = app.EnabledProtocols;
            }
            else
            {
                publication.FullPath = site.PhysicalPath;
            }

            // Calcular informações do diretório físico
            await CalculateDirectoryInfo(publication);

            return publication;
        }

        /// <summary>
        /// Calcula tamanho e data de modificação do diretório físico
        /// </summary>
        private async Task CalculateDirectoryInfo(IISBasedPublication publication)
        {
            try
            {
                if (Directory.Exists(publication.FullPath))
                {
                    var dirInfo = new DirectoryInfo(publication.FullPath);
                    publication.LastModified = dirInfo.LastWriteTime;

                    // Calcular tamanho de forma assíncrona
                    var sizeTask = Task.Run(() =>
                    {
                        try
                        {
                            return CalculateDirectorySize(dirInfo);
                        }
                        catch
                        {
                            return 0.0;
                        }
                    });

                    publication.SizeMB = await sizeTask;
                }
                else
                {
                    publication.SizeMB = 0.0;
                    publication.LastModified = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao calcular informações do diretório: {Path}", publication.FullPath);
                publication.SizeMB = 0.0;
                publication.LastModified = null;
            }
        }

        /// <summary>
        /// Calcula o tamanho de um diretório em MB
        /// </summary>
        private double CalculateDirectorySize(DirectoryInfo dirInfo)
        {
            try
            {
                long sizeBytes = dirInfo.GetFiles("*", SearchOption.AllDirectories)
                    .Sum(file => file.Length);
                
                return Math.Round(sizeBytes / (1024.0 * 1024.0), 2);
            }
            catch
            {
                return 0.0;
            }
        }

        // Método legado mantido para compatibilidade
        public async Task<PublicationInfo?> GetPublicationByNameAsync(string name)
        {
            var publications = await GetPublicationsAsync();
            var publication = publications.FirstOrDefault(p => 
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (publication == null) return null;

            return new PublicationInfo
            {
                Name = publication.Name,
                Repository = publication.RepoUrl ?? "N/A",
                Branch = publication.Branch ?? "N/A",
                BuildCommand = publication.BuildCommand ?? "N/A",
                FullPath = publication.FullPath,
                DeployedAt = publication.DeployedAt ?? DateTime.MinValue,
                Exists = publication.Exists,
                SizeMB = publication.SizeMB,
                LastModified = publication.LastModified ?? DateTime.MinValue
            };
        }
    }
}
