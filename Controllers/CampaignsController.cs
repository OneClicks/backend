using backend.DTOs;
using backend.Entities;
using backend.Services;
using backend.Services.Interfaces;
using backend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignsController : ControllerBase
    {
        private readonly IFB _facebookService;
        private readonly ILogger<CampaignsController> _logger;


        public CampaignsController(IFB facebookService, ILogger<CampaignsController> logger)
        {
            _facebookService = facebookService;
            _logger = logger;
        }

        [HttpPost("Create")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Users>>> CreateCampaign(CampaignDto campaign)
        {
            try
            {
                var data = await _facebookService.CreateCampaignAsync(campaign);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while registering the user.");
            }
        }
    }
}
