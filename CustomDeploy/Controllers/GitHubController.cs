using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomDeploy.Services;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("github")]
    [Authorize]
    public class GitHubController : ControllerBase
    {
        private readonly GitHubService _gitHubService;
        private readonly ILogger<GitHubController> _logger;

        public GitHubController(GitHubService gitHubService, ILogger<GitHubController> logger)
        {
            _gitHubService = gitHubService;
            _logger = logger;
        }

        /// <summary>
        /// Testa conectividade com GitHub
        /// </summary>
        /// <returns>Status da conectividade</returns>
        [HttpGet("test-connectivity")]
        public async Task<IActionResult> TestConnectivity()
        {
            try
            {
                _logger.LogInformation("Testando conectividade com GitHub");

                var result = await _gitHubService.TestGitHubConnectivityAsync();

                var response = new
                {
                    success = result.Success,
                    message = result.Message,
                    userInfo = result.UserInfo,
                    timestamp = DateTime.UtcNow
                };

                if (result.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar conectividade com GitHub");
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Valida um repositório GitHub
        /// </summary>
        /// <param name="repoUrl">URL do repositório</param>
        /// <returns>Informações do repositório</returns>
        [HttpPost("validate-repository")]
        public async Task<IActionResult> ValidateRepository([FromBody] ValidateRepositoryRequest request)
        {
            try
            {
                _logger.LogInformation("Validando repositório: {RepoUrl}", request.RepoUrl);

                if (string.IsNullOrWhiteSpace(request.RepoUrl))
                {
                    return BadRequest(new { message = "URL do repositório é obrigatória" });
                }

                var result = await _gitHubService.ValidateRepositoryAsync(request.RepoUrl);

                var response = new
                {
                    success = result.Success,
                    message = result.Message,
                    repositoryInfo = result.RepoInfo,
                    timestamp = DateTime.UtcNow
                };

                if (result.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar repositório: {RepoUrl}", request.RepoUrl);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Valida uma branch de um repositório
        /// </summary>
        /// <param name="request">Dados da requisição</param>
        /// <returns>Informações da branch</returns>
        [HttpPost("validate-branch")]
        public async Task<IActionResult> ValidateBranch([FromBody] ValidateBranchRequest request)
        {
            try
            {
                _logger.LogInformation("Validando branch: {Branch} do repositório: {RepoUrl}", request.Branch, request.RepoUrl);

                if (string.IsNullOrWhiteSpace(request.RepoUrl))
                {
                    return BadRequest(new { message = "URL do repositório é obrigatória" });
                }

                if (string.IsNullOrWhiteSpace(request.Branch))
                {
                    return BadRequest(new { message = "Nome da branch é obrigatório" });
                }

                var result = await _gitHubService.ValidateBranchAsync(request.RepoUrl, request.Branch);

                var response = new
                {
                    success = result.Success,
                    message = result.Message,
                    branchInfo = result.BranchInfo,
                    timestamp = DateTime.UtcNow
                };

                if (result.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar branch: {Branch} do repositório: {RepoUrl}", request.Branch, request.RepoUrl);
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }
    }

    public class ValidateRepositoryRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
    }

    public class ValidateBranchRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
    }
}
