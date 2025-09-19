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

        public async Task<List<User>> GetUsersByFavoriteProductAsync(string productUrl)
        {
            var users = await _repository.GetAllUsersAsync(); 
            return users.Where(u => u.FavoriteProducts.Any(f => f.ProductUrl == productUrl)).ToList();
        }
    }
}
