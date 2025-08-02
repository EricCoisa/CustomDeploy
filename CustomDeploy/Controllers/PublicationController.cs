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

        public PublicationController(ILogger<PublicationController> logger, PublicationService publicationService)
        {
            _logger = logger;
            _publicationService = publicationService;
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

                var publication = await _publicationService.GetPublicationByNameAsync(name);

                if (publication == null)
                {
                    return NotFound(new { message = $"Publicação '{name}' não encontrada" });
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

                var stats = new
                {
                    totalPublications = publications.Count,
                    totalSizeMB = Math.Round(publications.Sum(p => p.SizeMB), 2),
                    averageSizeMB = publications.Count > 0 ? Math.Round(publications.Average(p => p.SizeMB), 2) : 0,
                    latestPublication = publications.FirstOrDefault(),
                    oldestPublication = publications.LastOrDefault(),
                    largestPublication = publications.OrderByDescending(p => p.SizeMB).FirstOrDefault(),
                    smallestPublication = publications.OrderBy(p => p.SizeMB).FirstOrDefault()
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
    }
}
