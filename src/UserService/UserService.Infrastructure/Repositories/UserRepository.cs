using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var value = await _context.Users.ToListAsync();
            return value;
        }

        public async Task<User?> GetByIdAsync(Guid id) =>
            await _context.Users.Include(u => u.FavoriteProducts).FirstOrDefaultAsync(u => u.Id == id);

        public async Task<User?> GetByEmailAsync(string email) =>
            await _context.Users.Include(u => u.FavoriteProducts).FirstOrDefaultAsync(u => u.Email == email);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
