using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public string Email { get; set; } 
        public string PasswordHash { get; set; } 
        public DateTime CreatedAt { get; set; } 

        public ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();
    }
}
