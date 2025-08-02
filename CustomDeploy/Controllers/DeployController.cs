using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomDeploy.Models;
using CustomDeploy.Services;

namespace CustomDeploy.Controllers
{
    [ApiController]
    [Route("deploy")]
    [Authorize]
    public class DeployController : ControllerBase
    {
        private readonly ILogger<DeployController> _logger;
        private readonly DeployService _deployService;

        public DeployController(ILogger<DeployController> logger, DeployService deployService)
        {
            _logger = logger;
            _deployService = deployService;
        }

        [HttpPost]
        public async Task<IActionResult> Deploy([FromBody] DeployRequest request)
        {
            try
            {
                _logger.LogInformation("Deploy request received for repository: {RepoUrl}, Branch: {Branch}", 
                    request.RepoUrl, request.Branch);

                // Validação básica
                if (string.IsNullOrWhiteSpace(request.RepoUrl) ||
                    string.IsNullOrWhiteSpace(request.Branch) ||
                    string.IsNullOrWhiteSpace(request.BuildCommand) ||
                    string.IsNullOrWhiteSpace(request.BuildOutput) ||
                    string.IsNullOrWhiteSpace(request.TargetPath))
                {
                    return BadRequest(new { message = "All fields are required" });
                }

                // Executar o deploy usando o DeployService
                var result = await _deployService.ExecuteDeployAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        repository = request.RepoUrl,
                        branch = request.Branch,
                        buildCommand = request.BuildCommand,
                        buildOutput = request.BuildOutput,
                        targetPath = request.TargetPath,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogError("Deploy failed: {ErrorMessage}", result.Message);
                    return StatusCode(500, new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing deploy request");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
