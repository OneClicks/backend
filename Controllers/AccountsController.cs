using backend.DTOs;
using backend.Entities;
using backend.Services.Interfaces;
using backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountsController> _logger;
        private readonly IFB _facebookService;
        public AccountsController(IAccountService accountService,
            ILogger<AccountsController> Logger
,
            IFB facebookService)
        {
            _accountService = accountService;
            _logger = Logger;
            _facebookService = facebookService;
        }

        [HttpPost("register")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<Users>>> Signup(RegisterDto registerDto)
        {
            try
            {
                var data = await _accountService.SignUpService(registerDto);
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
     
        [HttpPost("login")]
        public async Task<ActionResult<ResponseVM<UserDto>>> Login(LoginDto loginDto)
        {
            try
            {
                var data = await _accountService.LoginService(loginDto);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while log in.");
            }

        }
        [HttpGet("verify")]
        public async Task<ActionResult<ResponseVM<UserDto>>> Verify([FromQuery] string token)
        {
            try
            {
                var data = await _accountService.VerificationService(token);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString().Substring(0, 50));
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while verifying the user.");
            }
        }

        [HttpPost("forgetPassword")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<UserDto>>> Forget(ForgetPasswordDto forgetPassword)
        {
            try
            {
                var data = await _accountService.ForgetPasswordService(forgetPassword);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while ForgetPassword Service.");
            }
        }

        [HttpPost("resetPassword")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult<ResponseVM<UserDto>>> Reset(PasswordResetDto resetDto)
        {
            try
            {
                var data = await _accountService.ResetPasswordService(resetDto);
                _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while resetting password");
            }
        }
      //  [HttpGet]
        //[Authorize(Policy = "ApiKeyPolicy")]

        [HttpGet("getAdAccountData")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<ActionResult> getAdAccountData([FromQuery]string accessToken)
        {
            try
            {
                var adAccountId = "1295877481040276";
               var res= await _facebookService.GetAdAccountsData(accessToken);
                //var res = await _facebookService.CreateCampaign(accessToken, adAccountId);

                return Ok(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
                return StatusCode(503, "An error occurred while creating a campaign");
            }
        }

        //[HttpPost("newPassword")]
        ////[Authorize(Policy = "ApiKeyPolicy")]
        //public async Task<ActionResult<ResponseVM<UserDto>>> NewPassword(NewPasswordDto resetDto)
        //{
        //    try
        //    {
        //        var data = await _accountService.NewPasswordService(resetDto);
        //        _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
        //        return StatusCode(503, "An error occurred while creating new password");
        //    }
        //}

        ////[Authorize]
        //[HttpPost("changePassword")]
        //public async Task<ActionResult<ResponseVM<UserDto>>> ChangePassword(ChangePasswordDto changePasswordDto)
        //{
        //    try
        //    {
        //        var data = await _accountService.ChangePasswordService(changePasswordDto);
        //        _logger.LogInformation($"Response Code: {data.StatusCode}\nResponse Message: {data.Message}");
        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        _logger.LogError($"Error Code: {503}\nError Message: {ex.ToString().Substring(0, 50)}");
        //        return StatusCode(503, "An error occurred while change password");
        //    }
        //}
    }
}
