using backend.DTOs;
using backend.Entities;
using backend.ViewModels;

namespace backend.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ResponseVM<Users>> SignUpService(RegisterDto registerDto);
        Task<ResponseVM<UserDto>> LoginService(LoginDto loginDto);
        Task<ResponseVM<UserDto>> VerificationService(string token);
        Task<ResponseVM<UserDto>> ForgetPasswordService(ForgetPasswordDto forgetPassword);
        Task<ResponseVM<UserDto>> ResetPasswordService(PasswordResetDto passwordReset);
        //Task<ResponseVM<UserDto>> ChangePasswordService(ChangePasswordDto passwordReset);
        //Task<ResponseVM<UserDto>> NewPasswordService(NewPasswordDto passwordReset);
    }
}
