using BusinessLayer.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelLayer.Entity;
using RepositoryLayer.Interface;
using RepositoryLayer.Repository;
using System.Security.Cryptography;
using System.Text;
using ModelLayer.Models.Auth;

namespace BusinessLayer.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        private readonly IJwtService _jwtService;
        public UserService(IUserRepository userRepo, IEmailService emailService, IPasswordHasher<UserEntity> passwordHasher, IJwtService jwtService)
        {
            _passwordHasher = passwordHasher;
            _userRepo = userRepo;
            _emailService = emailService;
            _jwtService = jwtService;
        }


        public async Task<UserRegistrationModel> RegisterUser(UserRegistrationModel registrationModel)
        { 

            UserEntity user = new UserEntity()
            {
                FirstName = registrationModel.FirstName,
                LastName = registrationModel.LastName,
                Email = registrationModel.Email,
                ContactNo = registrationModel.ContactNo,
            };

            user.Password = _passwordHasher.HashPassword(user, registrationModel.Password);

            await _userRepo.AddUser(user);

            return registrationModel;
        }

        public async Task<LoginResponseModel?> LoginUser(UserLoginModel loginModel)
        {
            var user = await _userRepo.GetUserByEmailId(loginModel.Email);

            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginModel.Password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            var token = _jwtService.GenerateToken(
                user.Id.ToString(),
                user.Email);

            return new LoginResponseModel
            {
                Token = token,
                ExpiryTimeUtc = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userRepo.GetUserByEmailId(email.Trim());
            
            if (user == null) return;

            // if valid email then
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

            user.PasswordResetTokenHash = tokenHash;
            user.PasswordResetTokenExpiryUtc = DateTime.UtcNow.AddMinutes(20);

            await _userRepo.UpdateUser(user);

            var body = $"""
                Hello,

                We received a request to reset your FundooNotes password.

                Your password-reset token is:

                {token}

                This token expires in 20 minutes and can be used only once.

                If you did not request this, you can ignore this email.
                """;

            await _emailService.SendEmailAsync(
                email,
                "Fundoo Notes - Password Reset",
                body);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordModel model)
        {
            if(string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return false;
            }

            if (model.NewPassword.Length < 8) return false;

            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(model.Token)));

            var user = await _userRepo.GetUserByResetTokenHash(tokenHash);

            if (user == null ||
            user.PasswordResetTokenExpiryUtc == null ||
            user.PasswordResetTokenExpiryUtc <= DateTime.UtcNow)
            {
                return false;
            }

            user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiryUtc = null;

            await _userRepo.UpdateUser(user);
            return true;
        }
    }
}
