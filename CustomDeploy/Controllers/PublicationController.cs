using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomDeploy.Models;
using CustomDeploy.Services;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("deploy")]
    [Authorize]
    public class PublicationController : ControllerBase
    {
        private readonly ILogger<PublicationController> _logger;
        private readonly PublicationService _publicationService;
        private readonly DeployService _deployService;
        private readonly IISManagementService _iisManagementService;

        public PublicationController(ILogger<PublicationController> logger, PublicationService publicationService, DeployService deployService, IISManagementService iisManagementService)
        {
            _logger = logger;
            _publicationService = publicationService;
            _deployService = deployService;
            _iisManagementService = iisManagementService;
        }

        /// <summary>
        /// Lista todas as publicações existentes
        /// </summary>
        /// <returns>Lista de publicações com informações detalhadas</returns>
        [HttpGet("publications")]
        public async Task<IActionResult> GetPublications()
        {
            try
            {
                _logger.LogInformation("Solicitação para listar publicações recebida");

                var publications = await _publicationService.GetPublicationsAsync();

                var response = new
                {
                    message = "Publicações listadas com sucesso (IIS como fonte de verdade)",
                    count = publications.Count,
                    publications = publications,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar publicações");
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém detalhes de uma publicação específica
        /// </summary>
        /// <param name="name">Nome da publicação</param>
        /// <returns>Detalhes da publicação</returns>
        [HttpGet("publications/{name}")]
        public async Task<IActionResult> GetPublicationByName(string name)
        {
            try
            {
                _logger.LogInformation("Solicitação para obter publicação: {Name}", name);

                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Nome da publicação é obrigatório" });
                }

                // Decodificar URL para suportar nomes com barras (ex: carteira%2Fapi -> carteira/api)
                var decodedName = Uri.UnescapeDataString(name);

                var publication = await _publicationService.GetPublicationByNameAsync(decodedName);

                if (publication == null)
                {
                    return NotFound(new { message = $"Publicação '{decodedName}' não encontrada" });
                }

                var response = new
                {
                    message = "Publicação encontrada",
                    publication = publication,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter publicação: {Name}", name);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém estatísticas gerais das publicações
        /// </summary>
        /// <returns>Estatísticas das publicações</returns>
        [HttpGet("publications/stats")]
        public async Task<IActionResult> GetPublicationsStats()
        {
            try
            {
                _logger.LogInformation("Solicitação para obter estatísticas das publicações");

                var publications = await _publicationService.GetPublicationsAsync();

                // Separar publicações com e sem metadados
                var withMetadata = publications.Where(p => p.HasMetadata).ToList();
                var withoutMetadata = publications.Where(p => !p.HasMetadata).ToList();
                var sites = publications.Where(p => p.SubApplication == null).ToList();
                var applications = publications.Where(p => p.SubApplication != null).ToList();

                var stats = new
                {
                    totalPublications = publications.Count,
                    sites = sites.Count,
                    applications = applications.Count,
                    withMetadata = withMetadata.Count,
                    withoutMetadata = withoutMetadata.Count,
                    totalSizeMB = Math.Round(publications.Sum(p => p.SizeMB), 2),
                    averageSizeMB = publications.Count > 0 ? Math.Round(publications.Average(p => p.SizeMB), 2) : 0,
                    latestDeployment = withMetadata.Where(p => p.DeployedAt.HasValue).OrderByDescending(p => p.DeployedAt).FirstOrDefault(),
                    oldestDeployment = withMetadata.Where(p => p.DeployedAt.HasValue).OrderBy(p => p.DeployedAt).FirstOrDefault(),
                    largestPublication = publications.OrderByDescending(p => p.SizeMB).FirstOrDefault(),
                    smallestPublication = publications.Where(p => p.SizeMB > 0).OrderBy(p => p.SizeMB).FirstOrDefault(),
                    sitesWithoutMetadata = withoutMetadata.Take(5).ToList()
                };

                var response = new
                {
                    message = "Estatísticas obtidas com sucesso (baseadas no IIS)",
                    stats = stats,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas das publicações");
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Remove uma publicação completamente (metadados e pasta física)
        /// </summary>
        /// <param name="name">Nome da publicação a ser removida</param>
        /// <returns>Resultado da operação de remoção</returns>
        [HttpDelete("publications/{name}")]
        public IActionResult DeletePublication(string name)
        {
            try
            {
                _logger.LogInformation("Solicitação para remover publicação completamente: {Name}", name);

                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Nome da publicação é obrigatório" });
                }

                // Decodificar URL para suportar nomes com barras (ex: carteira%2Fapi -> carteira/api)
                var decodedName = Uri.UnescapeDataString(name);
                _logger.LogInformation("Nome decodificado: {DecodedName}", decodedName);

                var result = _deployService.DeletePublicationCompletely(decodedName);

                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                var response = new
                {
                    message = result.Message,
                    name = decodedName,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover publicação completamente: {Name}", name);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Remove apenas os metadados de uma publicação (mantém a pasta física)
        /// </summary>
        /// <param name="name">Nome da publicação</param>
        /// <returns>Resultado da operação de remoção</returns>
        [HttpDelete("publications/{name}/metadata-only")]
        public IActionResult DeletePublicationMetadataOnly(string name)
        {
            try
            {
                _logger.LogInformation("Solicitação para remover apenas metadados da publicação: {Name}", name);

                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Nome da publicação é obrigatório" });
                }

                // Decodificar URL para suportar nomes com barras (ex: carteira%2Fapi -> carteira/api)
                var decodedName = Uri.UnescapeDataString(name);
                _logger.LogInformation("Nome decodificado: {DecodedName}", decodedName);

                var result = _deployService.RemoveDeployMetadata(decodedName);

                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                var response = new
                {
                    message = $"Metadados removidos (pasta física mantida): {result.Message}",
                    name = decodedName,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover metadados da publicação: {Name}", name);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém detalhes específicos de um deploy pelos metadados
        /// </summary>
        /// <param name="name">Nome do deploy</param>
        /// <returns>Metadados do deploy com status de existência atualizado</returns>
        [HttpGet("publications/{name}/metadata")]
        public IActionResult GetDeployMetadata(string name)
        {
            try
            {
                _logger.LogInformation("Solicitação para obter metadados do deploy: {Name}", name);

                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Nome do deploy é obrigatório" });
                }

                // Decodificar URL para suportar nomes com barras (ex: carteira%2Fapi -> carteira/api)
                var decodedName = Uri.UnescapeDataString(name);
                _logger.LogInformation("Nome decodificado: {DecodedName}", decodedName);

                var result = _deployService.GetDeployMetadata(decodedName);

                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                var response = new
                {
                    message = "Metadados encontrados",
                    metadata = result.Deploy,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metadados do deploy: {Name}", name);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza metadados específicos de uma publicação (Repository, Branch, BuildCommand)
        /// </summary>
        /// <param name="name">Nome da publicação</param>
        /// <param name="request">Dados para atualização</param>
        /// <returns>Metadados atualizados</returns>
        [HttpPatch("publications/{name}/metadata")]
        public IActionResult UpdatePublicationMetadata(string name, [FromBody] UpdateMetadataRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitação para atualizar metadados da publicação: {Name}", name);

                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Nome da publicação é obrigatório" });
                }

                if (request == null)
                {
                    return BadRequest(new { message = "Dados para atualização são obrigatórios" });
                }

                // Validar se pelo menos um campo foi fornecido
                if (string.IsNullOrWhiteSpace(request.Repository) && 
                    string.IsNullOrWhiteSpace(request.Branch) && 
                    string.IsNullOrWhiteSpace(request.BuildCommand))
                {
                    return BadRequest(new { 
                        message = "Pelo menos um campo deve ser fornecido para atualização",
                        allowedFields = new[] { "repository", "branch", "buildCommand" }
                    });
                }

                // Decodificar URL para suportar nomes com barras (ex: carteira%2Fapi -> carteira/api)
                var decodedName = Uri.UnescapeDataString(name);
                _logger.LogInformation("Nome decodificado: {DecodedName}", decodedName);

                var result = _deployService.UpdateDeployMetadata(
                    decodedName, 
                    request.Repository, 
                    request.Branch, 
                    request.BuildCommand);

                if (!result.Success)
                {
                    if (result.Message.Contains("não encontrado"))
                    {
                        return NotFound(new { message = result.Message });
                    }
                    return BadRequest(new { message = result.Message });
                }

                var response = new
                {
                    message = "Metadados atualizados com sucesso",
                    details = result.Message,
                    updatedMetadata = result.UpdatedDeploy,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar metadados da publicação: {Name}", name);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Cria metadados para uma publicação sem executar deploy
        /// </summary>
        /// <param name="request">Dados dos metadados a serem criados</param>
        /// <returns>Metadados criados</returns>
        [HttpPost("publications/metadata")]
        public async Task<IActionResult> CreateMetadados([FromBody] CreateMetadataRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitação para criar metadados: {IisSiteName}/{SubPath}", 
                    request.IisSiteName, request.SubPath ?? "");

                // Validar dados obrigatórios
                if (request == null)
                {
                    return BadRequest(new { message = "Dados são obrigatórios" });
                }

                if (string.IsNullOrWhiteSpace(request.IisSiteName))
                {
                    return BadRequest(new { message = "IisSiteName é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(request.RepoUrl))
                {
                    return BadRequest(new { message = "RepoUrl é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(request.Branch))
                {
                    return BadRequest(new { message = "Branch é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(request.BuildCommand))
                {
                    return BadRequest(new { message = "BuildCommand é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(request.BuildOutput))
                {
                    return BadRequest(new { message = "BuildOutput é obrigatório" });
                }

                // Verificar se o site existe no IIS
                var sitesResult = await _iisManagementService.GetAllSitesAsync();
                if (!sitesResult.Success)
                {
                    return StatusCode(500, new { message = "Erro ao verificar sites no IIS", details = sitesResult.Message });
                }

                var targetSite = sitesResult.Sites.FirstOrDefault(s => 
                {
                    var siteData = System.Text.Json.JsonSerializer.Serialize(s);
                    var siteInfo = System.Text.Json.JsonSerializer.Deserialize<SiteInfo>(siteData);
                    return string.Equals(siteInfo?.Name, request.IisSiteName, StringComparison.OrdinalIgnoreCase);
                });

                if (targetSite == null)
                {
                    return BadRequest(new { message = $"Site IIS '{request.IisSiteName}' não encontrado" });
                }

                // Deserializar informações do site
                var siteJson = System.Text.Json.JsonSerializer.Serialize(targetSite);
                var siteInfo = System.Text.Json.JsonSerializer.Deserialize<SiteInfo>(siteJson);

                if (siteInfo == null)
                {
                    return StatusCode(500, new { message = "Erro ao processar informações do site IIS" });
                }

                // Construir o caminho de destino
                string targetPath = siteInfo.PhysicalPath;
                string metadataName = request.IisSiteName;

                // Se há subPath, verificar se existe como aplicação no IIS
                if (!string.IsNullOrWhiteSpace(request.SubPath))
                {
                    var appsResult = await _iisManagementService.GetSiteApplicationsAsync(request.IisSiteName);
                    if (appsResult.Success)
                    {
                        var applications = appsResult.Data as List<object> ?? new List<object>();
                        var targetApp = applications.FirstOrDefault(a =>
                        {
                            var appData = System.Text.Json.JsonSerializer.Serialize(a);
                            var appInfo = System.Text.Json.JsonSerializer.Deserialize<ApplicationInfo>(appData);
                            var appPath = appInfo?.Name?.TrimStart('/');
                            return string.Equals(appPath, request.SubPath, StringComparison.OrdinalIgnoreCase);
                        });

                        if (targetApp != null)
                        {
                            // É uma aplicação IIS existente
                            var appJson = System.Text.Json.JsonSerializer.Serialize(targetApp);
                            var appInfo = System.Text.Json.JsonSerializer.Deserialize<ApplicationInfo>(appJson);
                            targetPath = appInfo?.PhysicalPath ?? Path.Combine(targetPath, request.SubPath);
                            metadataName = $"{request.IisSiteName}/{request.SubPath}";
                        }
                        else
                        {
                            // Não é aplicação IIS, será um subdiretório
                            targetPath = Path.Combine(targetPath, request.SubPath);
                            metadataName = $"{request.IisSiteName}/{request.SubPath}";
                        }
                    }
                    else
                    {
                        // Assumir como subdiretório
                        targetPath = Path.Combine(targetPath, request.SubPath);
                        metadataName = $"{request.IisSiteName}/{request.SubPath}";
                    }
                }

                // Verificar se já existe metadado para este nome/caminho
                var existingMetadata = _deployService.GetAllDeployMetadata();
                var duplicateByName = existingMetadata.FirstOrDefault(m => 
                    string.Equals(m.Name, metadataName, StringComparison.OrdinalIgnoreCase));

                var duplicateByPath = existingMetadata.FirstOrDefault(m => 
                    string.Equals(Path.GetFullPath(m.TargetPath), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase));

                if (duplicateByName != null)
                {
                    return BadRequest(new { 
                        message = $"Já existe um metadado registrado com o nome '{metadataName}'",
                        existingMetadata = duplicateByName
                    });
                }

                if (duplicateByPath != null)
                {
                    return BadRequest(new { 
                        message = $"Já existe um metadado registrado para o caminho '{targetPath}'",
                        existingMetadata = duplicateByPath
                    });
                }

                // Criar novo metadado usando o DeployService
                var metadataResult = _deployService.CreateMetadataOnly(
                    metadataName,
                    request.RepoUrl,
                    request.Branch,
                    request.BuildCommand,
                    targetPath
                );

                if (!metadataResult.Success)
                {
                    return BadRequest(new { message = metadataResult.Message });
                }

                _logger.LogInformation("Metadados criados com sucesso: {Name} -> {TargetPath}", 
                    metadataName, targetPath);

                var response = new
                {
                    repoUrl = request.RepoUrl,
                    branch = request.Branch,
                    buildCommand = request.BuildCommand,
                    buildOutput = request.BuildOutput,
                    targetPath = targetPath,
                    iisSiteName = request.IisSiteName,
                    subPath = request.SubPath,
                    exists = metadataResult.Metadata?.Exists ?? false,
                    name = metadataName,
                    timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar metadados");
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }
    }
}
