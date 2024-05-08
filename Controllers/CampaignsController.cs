using backend.DTOs;
using backend.Entities;
using backend.Service;
using backend.Service.Interfaces;
using backend.ViewModels;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2;
using Google.Rpc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using backend.Configurations;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignsController : ControllerBase
    {
        private readonly IFB _facebookService;
        private readonly ILogger<CampaignsController> _logger;
        private readonly HttpClient _httpClientFactory;
        private readonly IGoogleApiService googleApiService;

        public CampaignsController(IFB facebookService, ILogger<CampaignsController> logger, HttpClient httpClient, IGoogleApiService googleApiService)
        {
            _facebookService = facebookService;
            _logger = logger;
            _httpClientFactory = httpClient;
            this.googleApiService = googleApiService;
        }
        [HttpGet("test")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Users>>> Test(string code)
        {
            try
            {

                //    var clientSecrets = new ClientSecrets
                //    {
                //        ClientId = "195870252277-kgqnfto3d27fhvvhivk7m3ikfkc4qhvl.apps.googleusercontent.com",
                //        ClientSecret = "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"
                //    };

                //var tokenRequestContent = new MultipartFormDataContent
                //{
                //    { new StringContent(code), "code" },
                //    { new StringContent(clientSecrets.ClientId), "client_id" },
                //    { new StringContent(clientSecrets.ClientSecret), "client_secret" },
                //    { new StringContent("https://localhost:3000"), "redirect_uri" },
                //    { new StringContent("authorization_code"), "grant_type" }
                //};


                //var response = await _httpClientFactory.PostAsync("https://oauth2.googleapis.com/token", tokenRequestContent);
                //    var responseContent = await response.Content.ReadAsStringAsync();
               
                //using JsonDocument document = JsonDocument.Parse(responseContent);

                //var res = document.RootElement.GetProperty("refresh_token").GetString();

                //await googleApiService.GetAllCampaigns(2989534382);
                 //googleApiService.GetAccountHierarchy(4520819258);

                return Ok();
                    //var data = await _facebookService.GetCities("Un", "EAAKbj1ZAaEcgBOxoDtv1ZABZACPu4bQsi8u5OfypNAkCIieC9gp6VQQZAKqL1MeBgDZBhheEjsMnocD1LsD5kheGI4dZC9mDoYHbL9Fkwbp2K7HvEwFZB0nJn62O2EOwjCsGFHsH3JAjUVsVKCKPZAMZAM9sZA6MV8a9QbwlXLa5ulTvGcoX7GFiaW31QwjEW4bjEg1mSmZC63e2z8bo6GImQZDZD");
                    //_logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                    //return Ok(data);
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

        [HttpPost("CreateAd")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Adset>>> CreateAd(AdDto ad)
        {
            try
            {
                var accessToken = "EAAKbj1ZAaEcgBOykHcNeUAQcIwzGfShwTFzuvWSqIIGdHj61MKStA7qPQcPZCpFBgrUZAN4IJUgktKh1L7ILFWWEMUnZBr6OpUgSpMfZAEbKKTsK4MSJJ0QDWEp9fpt6u0tQaoEZBwNhmxpRgkvNdQjxBepjMs87LEAzUH28ZBZAXxHPlQogcLZARHTlgoeGBlHSuonlxCFzypqDmqlloZApEZD";
                var adAccountId = "1295877481040276";
                var adsetId = "120207945721040113";
                var adsetName = "My Ad Set for aftercred";
                var creativeId = "120207945749960113";

                var data = await _facebookService.ScheduleDelivery(accessToken, adAccountId, adsetId, adsetName, creativeId);
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
