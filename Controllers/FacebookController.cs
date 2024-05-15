using backend.DTOs;
using backend.Entities;
using backend.ServiceFiles;
using backend.ServiceFiles.Interfaces;
using backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Globalization;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacebookController : ControllerBase
    {
        private readonly IFB _facebookService;
        private readonly ILogger<FacebookController> _logger;

        public FacebookController(IFB facebookService, ILogger<FacebookController> logger)
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
        public async Task<IActionResult> GetAllCampaigns([FromQuery] string accessToken, [FromQuery] string adAccountId)
        {
            try
            {
                var data = await _facebookService.GetCampaignData(accessToken,adAccountId); 
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

/*        [HttpGet("GetAllAdsets")]
        public async Task<ActionResult<ResponseVM<Adset>>> GetAllAdsets()
        {
            try
            {
                var data = await _facebookService.GetAllAdsets();
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred fetching all categories");
            }
        }*/

        [HttpPost("CreateAdcreative")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<AdCreative>>> CreateAdCreative(AdCreativeDto creative)
        {
            try
            {
                var data = await _facebookService.CreateAdCreative(creative);
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

        [HttpPut("CreateAdImageHash/{adAccountId}")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<IActionResult> CreateAdImageHash([FromQuery] string accessToken, [FromForm] IFormFile imageFile, string adAccountId)
        {
            try
            {
                var data = await _facebookService.UploadFile(accessToken, imageFile, adAccountId);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while creating image hash.");
            }
        }
        [HttpGet("GetAllAdcreatives")]
        public async Task<ActionResult<ResponseVM<AdCreative>>> GetAllAdcreatives()
        {
            try
            {
                var data = await _facebookService.GetAllAdcreatives();
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred fetching all ad creatives");
            }
        }

        [HttpPost("ScheduleAd")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<IActionResult> CreateAd(AdDto ad)
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

        [HttpGet("GetInterests")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<IActionResult> GetInterests([FromQuery] string accessToken, [FromQuery] string interests)
        {
            try
            {
                var data = await _facebookService.GetInterests(accessToken, interests);
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

        //[Authorize(Policy = "ApiKeyPolicy")]
        [HttpGet("GetTargetingCategory")]
        public async Task<IActionResult> GetTargetingCategory([FromQuery] string accessToken, [FromQuery] string targetType)
        {
            try
            {
                var data = await _facebookService.SearchAdTargetingCategories(accessToken, targetType);
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
        //[Authorize(Policy = "ApiKeyPolicy")]
        [HttpGet("GetCities")]
        public async Task<IActionResult> GetCities([FromQuery] string accessToken, [FromQuery] string city)
        {
            try
            {
                var data = await _facebookService.GetCities(accessToken, city);
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

        [HttpGet("GetAllAdsPayload")]
        public async Task<IActionResult> GetAllAdsPayload([FromQuery] string accessToken, [FromQuery] string adAccountId)
        {
            try
            {
                var data = await _facebookService.GetPayloadForAd(accessToken, adAccountId);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while getting ads payload");
            }
        }

        [HttpGet("GetAllAdsets")]
        public async Task<IActionResult> GetAllAdsets([FromQuery] string accessToken, [FromQuery] string adAccountId)
        {
            try
            {
                var data = await _facebookService.GetAdSetData(accessToken, adAccountId);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while getting ads payload");
            }
        }
    }
}
