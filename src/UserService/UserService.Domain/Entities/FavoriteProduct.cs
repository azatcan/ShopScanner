using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Entities
{
    public class FavoriteProduct
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ProductUrl { get; set; } 

        public User User { get; set; } = null!;
    }
}
