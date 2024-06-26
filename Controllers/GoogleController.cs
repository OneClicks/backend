﻿using backend.DTOs.GoogleDtos;
using backend.Entities;
using backend.ServiceFiles.Interfaces;
using backend.ViewModels;
using Google.Apis.Auth.OAuth2.Requests;
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

        [HttpGet("GetRefreshToken")]
        public async Task<IActionResult> GetRefreshToken([FromQuery] string code)
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
        [HttpGet("RevokeToken/{code}")]
        public async Task<IActionResult> RevokeToken(string code)
        {
            try
            {
                var data = await _googleApiService.RevokeToken(code);
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
        [HttpGet("GetManagerAccounts")]
        public async Task<IActionResult> GetManagerAccounts([FromQuery] string refreshToken)
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
        [HttpGet("GetClientAccounts")]
        public async Task<IActionResult> GetClientAccounts([FromQuery] string refreshToken, [FromQuery] string customerId)
        {
            try
            {
                string idPart = customerId.Split('/')[1]; // Split the string by '/' and take the second part
                long id = long.Parse(idPart); // Parse the ID part as a long
                var data = await _googleApiService.GetAccountHierarchy(refreshToken, id);
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

        [HttpPost("CreateClientAccount")]
        public async Task<IActionResult> CreateClientAccount(ClientAccountDto dto)
        {
            try
            {
                string idPart = dto.CustomerId.Split('/')[1]; // Split the string by '/' and take the second part
                long id = long.Parse(idPart); // Parse the ID part as a long
                var data = await _googleApiService.CreateCustomer(dto.RefreshToken, id);
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

        [HttpPost("CreateCampaign")]
        public async Task<IActionResult> CreateCampaign(GoogleCampaignDto campaignDto)
        {
            try
            {
                var data = await _googleApiService.CreateCampaigns(campaignDto);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred creating campaign");
            }
        }

        [HttpGet("GetAllCampaigns")]
        public async Task<IActionResult> GetAllCampaigns([FromQuery] string refreshToken, [FromQuery] long customerId, [FromQuery] long managerId)
        {
            try
            {
                var data = await _googleApiService.GetAllCampaigns(refreshToken, customerId, managerId);
                //_logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred fetching all campaigns");
            }
        }


        [HttpPost("CreateAdGroup")]
        public async Task<IActionResult> CreateAdGroup(AdGroupDto Dto)
        {
            try
            {
                var data = await _googleApiService.CreateAdGroup(Dto);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred creating ad group");
            }
        }

        [HttpGet("GetAllAds")]
        public async Task<IActionResult> GetAllAds([FromQuery] string refreshToken, [FromQuery] long customerId , [FromQuery] long managerId)
        {
            try
            {
                var data = await _googleApiService.GetAllResponseAds(refreshToken, customerId, managerId);
                //_logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred fetching all ads");
            }
        }


    }
}
