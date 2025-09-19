using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Entities;

namespace UserService.Domain.Services
{
    public interface IUserService
    {
        Task<List<User>> GetUsersByFavoriteProductAsync(string productUrl);
    }
}
