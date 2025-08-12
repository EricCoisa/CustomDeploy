using CustomDeploy.Models;
using CustomDeploy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        /// Lista todas as publicações baseadas no IIS
        /// </summary>
        /// <returns>Lista de publicações do IIS</returns>
        [HttpGet("publications")]
        public async Task<IActionResult> GetPublications()
        {
            try
            {
                _logger.LogInformation("Solicitação para listar todas as publicações");
                var publications = await _publicationService.GetPublicationsAsync();
                
                _logger.LogInformation("Retornando {Count} publicações", publications.Count);
                return Ok(publications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar publicações");
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém informações de uma publicação específica
        /// </summary>
        /// <param name="name">Nome da publicação</param>
        /// <returns>Informações da publicação</returns>
        [HttpGet("publications/{name}")]
        public async Task<IActionResult> GetPublication(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Nome da publicação é obrigatório" });
                }

                // Decodificar URL para suportar nomes com barras
                var decodedName = Uri.UnescapeDataString(name);
                _logger.LogInformation("Buscando publicação: {Name}", decodedName);

                var publication = await _publicationService.GetPublicationByNameAsync(decodedName);
                
                if (publication == null)
                {
                    return NotFound(new { message = $"Publicação '{decodedName}' não encontrada" });
                }

                return Ok(publication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar publicação: {Name}", name);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém estatísticas das publicações
        /// </summary>
        /// <returns>Estatísticas das publicações</returns>
        [HttpGet("publications/stats")]
        public async Task<IActionResult> GetPublicationsStats()
        {
            try
            {
                _logger.LogInformation("Solicitação para obter estatísticas das publicações");
                
                var publications = await _publicationService.GetPublicationsAsync();
                
                var stats = new
                {
                    total = publications.Count,
                    sites = publications.Where(p => string.IsNullOrEmpty(p.SubApplication)).Count(),
                    applications = publications.Where(p => !string.IsNullOrEmpty(p.SubApplication)).Count(),
                    withMetadata = publications.Count(p => !string.IsNullOrEmpty(p.RepoUrl)),
                    totalSizeMB = Math.Round(publications.Sum(p => p.SizeMB), 2),
                    lastUpdated = DateTime.UtcNow
                };
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas das publicações");
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Remove uma publicação completamente (pasta física)
        /// </summary>
        /// <param name="name">Nome da publicação a ser removida</param>
        /// <returns>Resultado da operação de remoção</returns>
        [HttpDelete("publications/{name}")]
        public async Task<IActionResult> DeletePublication(string name)
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

                // Buscar a publicação pelo nome
                var publication = await _publicationService.GetPublicationByNameAsync(decodedName);
                if (publication == null)
                {
                    return NotFound(new { message = $"Publicação '{decodedName}' não encontrada" });
                }

                // Tentar deletar a pasta física se existir
                var targetPath = publication.FullPath;
                var physicalDeletionSuccess = false;
                var physicalDeletionMessage = "";

                if (Directory.Exists(targetPath))
                {
                    try
                    {
                        _logger.LogInformation("Deletando pasta física: {TargetPath}", targetPath);
                        Directory.Delete(targetPath, true);
                        physicalDeletionSuccess = true;
                        physicalDeletionMessage = "Pasta física deletada com sucesso";
                        _logger.LogInformation("Pasta física deletada: {TargetPath}", targetPath);
                    }
                    catch (Exception ex)
                    {
                        physicalDeletionMessage = $"Erro ao deletar pasta física: {ex.Message}";
                        _logger.LogError(ex, "Erro ao deletar pasta física: {TargetPath}", targetPath);
                        return StatusCode(500, new { message = physicalDeletionMessage });
                    }
                }
                else
                {
                    physicalDeletionSuccess = true;
                    physicalDeletionMessage = "Pasta física não existe (já removida ou não encontrada)";
                    _logger.LogInformation("Pasta física não encontrada: {TargetPath}", targetPath);
                }

                var response = new
                {
                    message = $"Publicação '{decodedName}' removida com sucesso. {physicalDeletionMessage}",
                    name = decodedName,
                    physicalPath = targetPath,
                    physicalDeletionSuccess = physicalDeletionSuccess,
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
        /// Executa um redeploy de uma publicação existente
        /// </summary>
        /// <param name="name">Nome da publicação</param>
        /// <returns>Resultado do redeploy</returns>
        [HttpPost("publications/{name}/redeploy")]
        public async Task<IActionResult> RedeployPublication(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Nome da publicação é obrigatório" });
                }

                var decodedName = Uri.UnescapeDataString(name);
                _logger.LogInformation("Solicitação de redeploy para: {Name}", decodedName);

                // Buscar a publicação
                var publication = await _publicationService.GetPublicationByNameAsync(decodedName);
                if (publication == null)
                {
                    return NotFound(new { message = $"Publicação '{decodedName}' não encontrada" });
                }

                // Verificar se tem informações de repositório
                if (string.IsNullOrWhiteSpace(publication.Repository) || 
                    publication.Repository == "N/A")
                {
                    return BadRequest(new { 
                        message = "Publicação não possui informações de repositório para redeploy",
                        name = decodedName 
                    });
                }

                return BadRequest(new { 
                    message = "Funcionalidade de redeploy ainda não implementada após migração para banco de dados",
                    name = decodedName 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante redeploy da publicação: {Name}", name);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }
    }
}
