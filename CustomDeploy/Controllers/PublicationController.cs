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

        public PublicationController(ILogger<PublicationController> logger, PublicationService publicationService, DeployService deployService)
        {
            _logger = logger;
            _publicationService = publicationService;
            _deployService = deployService;
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
                    message = "Publicações listadas com sucesso",
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

                // Separar publicações existentes e removidas
                var existingPublications = publications.Where(p => p.Exists).ToList();
                var removedPublications = publications.Where(p => !p.Exists).ToList();

                var stats = new
                {
                    totalPublications = publications.Count,
                    existingPublications = existingPublications.Count,
                    removedPublications = removedPublications.Count,
                    totalSizeMB = Math.Round(existingPublications.Sum(p => p.SizeMB), 2), // Apenas apps existentes
                    averageSizeMB = existingPublications.Count > 0 ? Math.Round(existingPublications.Average(p => p.SizeMB), 2) : 0,
                    latestPublication = publications.FirstOrDefault(),
                    oldestPublication = publications.LastOrDefault(),
                    largestPublication = existingPublications.OrderByDescending(p => p.SizeMB).FirstOrDefault(),
                    smallestPublication = existingPublications.Where(p => p.SizeMB > 0).OrderBy(p => p.SizeMB).FirstOrDefault(),
                    recentlyRemoved = removedPublications.OrderByDescending(p => p.LastModified).Take(5).ToList()
                };

                var response = new
                {
                    message = "Estatísticas obtidas com sucesso",
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
    }
}
