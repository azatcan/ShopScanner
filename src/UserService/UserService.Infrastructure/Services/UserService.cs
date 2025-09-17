using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.Services;

namespace UserService.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<User> RegisterAsync(string name, string email, string password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Name = name, Email = email, PasswordHash = hash };
            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();
            return user;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _repository.GetByEmailAsync(email);
            if (user == null) return null;
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
        }

        public async Task<List<User>> GetUsersByFavoriteProductAsync(string productUrl)
        {
            var users = await _repository.GetAllUsersAsync(); 
            return users.Where(u => u.FavoriteProducts.Any(f => f.ProductUrl == productUrl)).ToList();
        }
    }
}
