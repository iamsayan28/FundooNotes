using ModelLayer.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interface
{
    public interface IUserService
    {
        public Task<LoginResponseModel?> LoginUser(UserLoginModel loginModel);
        public Task<UserRegistrationModel> RegisterUser(UserRegistrationModel registrationModel);
        public Task ForgotPasswordAsync(string email);
        public Task<bool> ResetPasswordAsync(ResetPasswordModel model);
    }
}
