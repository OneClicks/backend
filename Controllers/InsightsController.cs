using backend.DTOs;
using backend.Entities;
using backend.ServiceFiles.Interfaces;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsightsController: ControllerBase
    {
        private readonly ILogger<CampaignsController> _logger;
        private readonly IFB _facebookService;
        public InsightsController(ILogger<CampaignsController> logger, IFB facebookService)
        {
            _logger = logger;
            _facebookService = facebookService;
        }

        [HttpPost("test")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Users>>> Test()
        {
            try
            {
             //   var data = await _facebookService.GetCampaignInsights("120207538600830298", "EAAKbj1ZAaEcgBOZBRsk2WcOcIJNMhSmKtwaIIG87ltd8HyZCZAd6ZAe0AF79aXtHnsoicZCeLyoZBlwvStGMoQ4kBdukUIBM5ZCYpDmhve7TGOZA59XwPptsIZAXkTm8mxl62SiuUiKyqaOc5Wq2mLGKCuYO4nQZBphrHmoGZAr8CRLFYz4RPQZBRq9svNzrNTS3TtY3d6ACQvEYn1c0mHX8WN9EZD");
               // _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");

                var data = await _facebookService.GetAdAccountInsights("575670381167089", "EAAKbj1ZAaEcgBOZBRsk2WcOcIJNMhSmKtwaIIG87ltd8HyZCZAd6ZAe0AF79aXtHnsoicZCeLyoZBlwvStGMoQ4kBdukUIBM5ZCYpDmhve7TGOZA59XwPptsIZAXkTm8mxl62SiuUiKyqaOc5Wq2mLGKCuYO4nQZBphrHmoGZAr8CRLFYz4RPQZBRq9svNzrNTS3TtY3d6ACQvEYn1c0mHX8WN9EZD");
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");

                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while getting insights");
            }
        }


    }
}
