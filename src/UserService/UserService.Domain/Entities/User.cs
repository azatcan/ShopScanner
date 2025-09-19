using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
       public ICollection<FavoriteProduct> FavoriteProducts { get; set; } 
    }
}
