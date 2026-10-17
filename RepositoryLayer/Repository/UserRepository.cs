using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelLayer.Models;
using RepositoryLayer.Context;
using ModelLayer.Entity;
using RepositoryLayer.Interface;

namespace RepositoryLayer.Repository
{
    public class UserRepository : IUserRepository
    {
        private FundooDbContext _dbContext;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        public UserRepository(FundooDbContext dbContext, IPasswordHasher<UserEntity> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserEntity?> GetUserByEmailId(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserEntity?> GetUserById(int id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddUser(UserEntity user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateUser(UserEntity user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserEntity?> GetUserByResetTokenHash(string token)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.PasswordResetTokenHash == token);
        }
    }
}
