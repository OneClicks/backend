using backend.DTOs;
using backend.Entities;
using backend.ServiceFiles;
using backend.ServiceFiles.Interfaces;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsightsController: ControllerBase
    {
        private readonly ILogger<InsightsController> _logger;
        private readonly InsightsService _insightsService;
        public InsightsController(ILogger<InsightsController> logger, InsightsService insightsService)
        {
            _logger = logger;
            _insightsService = insightsService;
        }


        [HttpGet("GetBudgetAmountFacebook")]
        public async Task<IActionResult> GetTargetingCategory([FromQuery] string accessToken, [FromQuery] string adAccountId)
        {
            try
            {
                var data = await _insightsService.GetBudgetAmountFacebook(accessToken, adAccountId);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while getting interests");
            }
        }

        [HttpGet("GetRecentActivity")]
        public async Task<IActionResult> GetRecentActivity([FromQuery] string accessToken, [FromQuery] string adAccountId)
        {
            try
            {
                var data = await _insightsService.GetBudgetAmountFacebook(accessToken, adAccountId);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while getting interests");
            }
        }
    }
}
