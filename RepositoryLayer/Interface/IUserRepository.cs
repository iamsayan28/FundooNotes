using ModelLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IUserRepository
    {
        //public UserRegistrationModel RegisterUser(UserRegistrationModel registrationModel);
        //public Task<bool> LoginUser(UserLoginModel loginModel);
        public Task<UserEntity?> GetUserByEmailId(string email);
        public Task<UserEntity?> GetUserById(int id);

        public Task UpdateUser(UserEntity user);
        public Task<UserEntity?> GetUserByResetTokenHash(string token);
        public Task AddUser(UserEntity user);
    }
}
