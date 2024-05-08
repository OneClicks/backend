using backend.Entities;
using backend.Service.Interfaces;
using backend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly IGoogleApiService _googleApiService;
        private readonly ILogger<GoogleController> _logger;


        public GoogleController(IGoogleApiService googleApiService, ILogger<GoogleController> logger)
        {
            _googleApiService = googleApiService;
            _logger = logger;
        }

        [HttpGet("GetRefreshToken/{code}")]
        public async Task<IActionResult> GetAllCampaigns(string code)
        {
            try
            {
                var data = await _googleApiService.GetRefreshToken(code);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred fetching all categories");
            }
        }
        [HttpGet("GetManagerAcccounts")]
        public async Task<IActionResult> GetAccountHierarchy(string refreshToken)
        {
            try
            {
                var data = await _googleApiService.GetAccessibleAccounts(refreshToken);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred fetching all categories");
            }
        }

    }
}
