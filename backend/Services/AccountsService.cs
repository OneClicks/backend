using backend.DTOs;
using backend.Entities;
using backend.Helpers;
using backend.Repository.Interfaces;
using backend.Services.API.Services;
using backend.Services.Interfaces;
using backend.ViewModels;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

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

            var user = new Users
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Username = registerDto.Username.ToLower(),
                Email = registerDto.Email,

                //Role = registerDto.Role,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(_randomPassword)),
                PasswordSalt = hmac.Key,


                VerificationToken = HelperFunctions.CreateRandomToken(),

                VerifiedAt = DateTime.UtcNow,
                Status = "Verified"
            };

            // Insert the user into MongoDB
            user = await _userRepository.Create(user);

            // Sending Email
            var message = new Message(new string[] { registerDto.Email }, "OneClicks User Credentials", "Hello OneClicks User " +
                "(" + registerDto.FirstName + " " + registerDto.LastName + "), \nYour account has been created. Your login credentials are as follows:" +
                "\nUsername: " + registerDto.Username.ToLower() + "\nPassword: " + _randomPassword + "\n\n" +
                "After logging in you will need to set your new password.");
            if (_emailService.SendEmail(message))
            {
                var filterUser = Builders<Users>.Filter.Eq("Id", user.Id);
                var update = Builders<Users>.Update
                    .Set(u => u.VerificationToken, HelperFunctions.CreateRandomToken())
                    .Set(u => u.VerifiedAt, DateTime.UtcNow);
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

            if (user.LastChangePassword == null)
                return new ResponseVM<UserDto>("250", "New Password Need To Be Set!");

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


        //public Task<ViewModels.ResponseVM<UserDto>> ChangePasswordService(ChangePasswordDto passwordReset)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<ViewModels.ResponseVM<UserDto>> ForgetPasswordService(ForgetPasswordDto forgetPassword)
        //{
        //    throw new NotImplementedException();
        //}



        //public Task<ViewModels.ResponseVM<UserDto>> NewPasswordService(NewPasswordDto passwordReset)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<ViewModels.ResponseVM<UserDto>> ResetPasswordService(PasswordResetDto passwordReset)
        //{
        //    throw new NotImplementedException();
        //}



        //public Task<ViewModels.ResponseVM<UserDto>> VerificationService(Token token)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
