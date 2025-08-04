

using MyCustomApp.Domain.Modules.Game;
using Microsoft.EntityFrameworkCore;

namespace MyCustomApp.Application.Contracts
{
    public interface IGameDbContext: IDbContext
    {
        DbSet<Game> Games { get; set; }
    }
}
