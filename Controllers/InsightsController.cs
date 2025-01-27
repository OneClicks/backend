﻿using backend.DTOs;
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

        [HttpPost("test")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Users>>> Test(CampaignDto campaign)
        {
            try
            {
                _insightsService.AddRecentActivity();

                return Ok();
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while creating campaigns.");
            }
        }
        [HttpGet("GetBudgetAmountFacebook")]
        public async Task<IActionResult> GetBudgetAmountFacebook([FromQuery] string accessToken, [FromQuery] string adAccountId)
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
        [HttpGet("GetBudgetAmountGoogle")]
        public async Task<IActionResult> GetBudgetAmountGoogle([FromQuery] string accessToken, [FromQuery] string customer, [FromQuery] string manager)
        {
            try
            {
                var data = await _insightsService.GetBudgetAmountGoogle(accessToken,customer, manager );
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
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                var data = await _insightsService.GetRecentActivity();
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
