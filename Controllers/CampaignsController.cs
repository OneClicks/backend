using backend.DTOs;
using backend.Entities;
using backend.Services;
using backend.Services.Interfaces;
using backend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Globalization;

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
        [HttpPost("test")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Users>>> Test(CampaignDto campaign)
        {
            try
            {
                var data = await _facebookService.GetCities("Un", "EAAKbj1ZAaEcgBOxoDtv1ZABZACPu4bQsi8u5OfypNAkCIieC9gp6VQQZAKqL1MeBgDZBhheEjsMnocD1LsD5kheGI4dZC9mDoYHbL9Fkwbp2K7HvEwFZB0nJn62O2EOwjCsGFHsH3JAjUVsVKCKPZAMZAM9sZA6MV8a9QbwlXLa5ulTvGcoX7GFiaW31QwjEW4bjEg1mSmZC63e2z8bo6GImQZDZD");
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while creating campaigns.");
            }
        }


        [HttpPost("CreateCampaign")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Users>>> CreateCampaign(CampaignDto campaign)
        {
            try
            {
                var data = await _facebookService.CreateCampaign(campaign);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while creating campaigns.");
            }
        }

        [HttpGet("GetAllCampaigns")]
        public async Task<ActionResult<ResponseVM<Campaigns>>> GetAllCampaigns()
        {
            try
            {
                var data = await _facebookService.GetAllCampaigns(); 
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
        [HttpPost("CreateAdset")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Adset>>> CreateAdset(AdsetDto adset)
        {
            try
            {
                var data = await _facebookService.CreateAdSet(adset);
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

        [HttpPost("CreateAdcreative")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Adset>>> CreateAdCreative(AdCreativeDto creative )
        {
            try
            {
                creative.ImageFile = "D:\\TestPicture\\picture2.png";
                var data = await _facebookService.ProvideAdCreative(creative);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while creating adcreative.");
            }
        }

        [HttpPost("ScheduleAd")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Adset>>> CreateAd(AdDto ad)
        {
            try
            {
                var data = await _facebookService.ScheduleDelivery(ad);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while creating ad.");
            }
        }
    }
}
