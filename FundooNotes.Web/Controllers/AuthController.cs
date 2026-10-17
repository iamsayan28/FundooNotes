using BusinessLayer.Interface;
using BusinessLayer.Service;
using FundooNotes.Web.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelLayer.Models;
using ModelLayer.Models.Auth;
using RepositoryLayer.Context;

namespace FundooNotes.Web.Controllers
{
    [ApiController]
    [Route("api/Fundoo")]
    public class AuthController : ControllerBase
    {
        private IUserService _userService;
        private IEmailService _emailService;
        public AuthController(IUserService userService, IEmailService emailService)
        {
            _emailService = emailService;
            _userService = userService;
        }

        public string Welcome() 
        {
            return "Welcome to fundoo app";
        }

        [Authorize]
        [HttpGet("auth-test")]
        public IActionResult AuthTest()
        {
            return Ok("JWT authentication succeeded.");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterUser(UserRegistrationModel registrationModel)
        {
            var result = await _userService.RegisterUser(registrationModel);

            if (result != null)
            {
                return Ok(new ResponseModel<UserRegistrationModel>()
                {
                    Success = true,
                    Message = "User registered successfully",
                    Data = result
                });
            }
            else
            {
                return BadRequest(new ResponseModel<UserRegistrationModel>()
                {
                    Success = false,
                    Message = "User registration failed",
                    Data = null
                });
            }

        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginUser(UserLoginModel loginModel)
        {
            var result = await _userService.LoginUser(loginModel);


            if (result == null)
            {
                return Unauthorized(new ResponseModel<LoginResponseModel>()
                {
                    Success = false,
                    Message = "Invalid email or password",
                });
            }
            else
            {
                return Ok(new ResponseModel<LoginResponseModel>()
                {
                    Success = true,
                    Message = "Login successful",
                    Data = result
                });
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string to)
        {
            await _emailService.SendEmailAsync(
                to,
                "FundooNotes SMTP Test",
                "Success! Your FundooNotes application can send emails.");

            return Ok("Test email sent successfully.");
        }

        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordModel model)
        {
            await _userService.ForgotPasswordAsync(model.Email);

            return Ok(new ResponseModel<string>()
            {
                Success = true,
                Message = "Reset password token send",
                Data = model.Email
            });
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordModel model)
        {
            bool success = await _userService.ResetPasswordAsync(model);
            
            if(success)
            {
                return Ok(new ResponseModel<string>()
                {
                    Success = true,
                    Message = "Password reset successfully"
                });
            } else
            {
                return BadRequest(new ResponseModel<string>()
                {
                    Success = false,
                    Message = "Password reset failed, due to invalid password, token or expired token",
                });
            }
        }
    }           
}
