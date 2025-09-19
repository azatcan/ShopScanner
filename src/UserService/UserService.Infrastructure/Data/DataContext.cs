using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data
{
    public class DataContext : IdentityDbContext<User,AppRole,Guid>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FavoriteProduct>()
                .HasOne(f => f.User)
                .WithMany(u => u.FavoriteProducts)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        public DbSet<FavoriteProduct> FavoriteProducts { get; set; }
    }
}
