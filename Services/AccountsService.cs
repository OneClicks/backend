using backend.DTOs;
using backend.Entities;
using backend.Helpers;
using backend.Repository.Interfaces;
using backend.Services.API.Services;
using backend.Services.Interfaces;
using backend.ViewModels;
using MongoDB.Driver;
using Org.BouncyCastle.Crypto.Macs;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace backend.Services
{
    public class AccountsService : IAccountService
    {
        private readonly IGenericRepository<Users> _userRepository;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        public AccountsService(
            IGenericRepository<Users> userRepo,

            ITokenService tokenService,
            IEmailService emailService)
        {
            _userRepository = userRepo;

            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<ResponseVM<Users>> SignUpService(RegisterDto registerDto)
        {
            using var hmac = new HMACSHA512();
            var _randomPassword = HelperFunctions.CreateRandomPassword();
            var vToken = HelperFunctions.CreateRandomToken();
            var user = new Users
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Username = registerDto.Username.ToLower(),
                Email = registerDto.Email,
                //Role = registerDto.Role,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key,
               

            };

            // Insert the user into MongoDB
            user = await _userRepository.Create(user);
            string clientUrl = "https://oneclicks.azurewebsites.net/account/verify";

            string VerifyUrl = $"{clientUrl}?token={HttpUtility.UrlEncode(vToken)}";
            // Sending Email
            string emailBody = $"OneClicks User Credentials\nHello OneClicks User ({registerDto.FirstName} {registerDto.LastName}),\nYour account has been created. Your login credentials are as follows:\nUsername: {registerDto.Username.ToLower()}\nPassword: {registerDto.Password}\n\nBefore logging in, you will need to verify yourself by clicking this link: {VerifyUrl}.";

            var message = new Message(new string[] { registerDto.Email },"OneClicks - Verification", emailBody );



            if (_emailService.SendEmail(message))
            {
                var filterUser = Builders<Users>.Filter.Eq("Id", user.Id);
                var update = Builders<Users>.Update
                    .Set(u => u.VerificationToken, vToken)
                    .Set(u => u.Status, "Pending");
                await _userRepository.UpdateOnly(filterUser, update);
            }

            return new ResponseVM<Users>("200", "Registered Successfully!");
        }


        public async Task<ResponseVM<UserDto>> LoginService(LoginDto loginDto)
        {
            var filter = Builders<Users>.Filter.Eq("Username", loginDto.UserName.ToLower());

            var user = await _userRepository.ExistsOnly(filter);
            if (user == null)
                return new ResponseVM<UserDto>("400", "User not found!");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            // Compare the hashed new password with the hashed default password
            if (!Enumerable.SequenceEqual(ComputedHash, user.PasswordHash))
            {
                return new ResponseVM<UserDto>("401", "User not authorized!");
            }

            if (user.Status != "Verified" )
                return new ResponseVM<UserDto>("450", "User not activated!");

            //if (user.LastChangePassword == null)
            //    return new ResponseVM<UserDto>("250", "New Password Need To Be Set!");

            if (user.VerifiedAt == null)
                return new ResponseVM<UserDto>("401", "User not authorized!");


            var response = new UserDto
            {
                UserName = user.Username,
                Token = _tokenService.CreateToken(user.Username),
                _id = user.Id.ToString(),
                firstName = user.FirstName,
                lastName = user.LastName
            };
            return new ResponseVM<UserDto>("200", "Successfully Login!", response);
        }
        public async Task<ResponseVM<UserDto>> VerificationService(string token)
        {
            var filter = Builders<Users>.Filter.Eq("VerificationToken", token);

            var user = await _userRepository.ExistsOnly(filter);

            if (user == null)
                return new ResponseVM<UserDto>("400", "Bad Request: Invalid Token!");

            var update = Builders<Users>.Update
                .Set(u => u.VerifiedAt, DateTime.UtcNow)
                .Set(u => u.Status, "Verified");

            await _userRepository.UpdateOnly(filter, update);

            return new ResponseVM<UserDto>("200", "Successfully verified!");
        }

        

        public async Task<ResponseVM<UserDto>> ForgetPasswordService(ForgetPasswordDto forgetPassword)
        {

            var filter = Builders<Users>.Filter.Eq("Username", forgetPassword.Username.ToLower());
            var user = await _userRepository.ExistsOnly(filter);

            if (user == null)
                return new ResponseVM<UserDto>("400", "Bad Request: User Not Found!");
            string token = HelperFunctions.CreateRandomToken();

            var update = Builders<Users>.Update.Set(u => u.PasswordResetToken, token)
                .Set(u => u.PasswordResetTokenExpires, DateTime.UtcNow.AddHours(7));

            var response = await _userRepository.UpdateOnly(filter, update);
            if (response.ModifiedCount > 0)
            {
                string clientUrl = "https://oneclicks.azurewebsites.net/account/reset-password";

                string resetPasswordUrl = $"{clientUrl}?token={HttpUtility.UrlEncode(token)}";

                string emailBody = $"Dear One User,\n\nPlease click on the following link to reset your password:" +
                    $"\n\n{resetPasswordUrl}" +
                    $"\n\nIf you did not request a password reset, please ignore this email.";

                var message = new Message(new string[] { user.Email }, "MBOT Reset Password", emailBody);
                if (_emailService.SendEmail(message))
                    return new ResponseVM<UserDto>("200", "Reset link is sent to your email address!\nYou can now reset your password!");
                else
                    return new ResponseVM<UserDto>("400", "Bad Request: Email not Sent!");
            }
            else
                return new ResponseVM<UserDto>("400", "Bad Request: Unable to generate reset password link!");
        }




        public  async Task<ResponseVM<UserDto>> ResetPasswordService(PasswordResetDto passwordReset)
        {
            var filter = Builders<Users>.Filter.Eq("PasswordResetToken", passwordReset.Token);
            var user = await _userRepository.ExistsOnly(filter);

            if (user == null)
                return new ResponseVM<UserDto>("400", "Bad Request: Invalid Token!");

            if (user.PasswordResetTokenExpires < DateTime.UtcNow)
                return new ResponseVM<UserDto>("401", "Bad Request: Token is Expired!");

            using var hmac = new HMACSHA512();

            var update = Builders<Users>.Update
                .Set(u => u.PasswordResetToken, null)
                .Set(u => u.PasswordResetTokenExpires, null)
                .Set(u => u.PasswordHash, hmac.ComputeHash(Encoding.UTF8.GetBytes(passwordReset.Password)))
                .Set(u => u.PasswordSalt, hmac.Key);

            var response = await _userRepository.UpdateOnly(filter, update);
            if (response.ModifiedCount > 0)
                return new ResponseVM<UserDto>("200", "Successfully Reset Password!");
            else
                return new ResponseVM<UserDto>("400", "Bad Request: Unable to reset password!");
        }



    }
}
