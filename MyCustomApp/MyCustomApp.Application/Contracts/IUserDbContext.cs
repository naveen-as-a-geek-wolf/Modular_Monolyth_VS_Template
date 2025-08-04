using MyCustomApp.Domain.Modules.User;
using Microsoft.EntityFrameworkCore;

namespace MyCustomApp.Application.Contracts
{
    public interface IUserDbContext : IDbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
