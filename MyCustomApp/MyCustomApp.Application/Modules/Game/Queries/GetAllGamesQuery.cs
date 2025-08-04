using MyCustomApp.Application.Contracts;
using MyCustomApp.Application.Modules.Game.Commands.CreateGame;
using LanguageExt.Common;
using MediateX.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MyCustomApp.Application.Modules.Game.Queries;

public record GetAllGamesQuery() : IQuery<Result<List<GameCreatedResult>>>;

public class GetAllGamesQueryHandler : IQueryHandler<GetAllGamesQuery,Result<List<GameCreatedResult>>>
{
    private readonly IGameDbContext _dbContext;

    public GetAllGamesQueryHandler(IGameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<GameCreatedResult>>> Handle(GetAllGamesQuery query, CancellationToken cancellationToken = default)
    {
        var games = await _dbContext.Games.ToListAsync(cancellationToken);
        return games.Select(g => new GameCreatedResult(g.Id, g.Name, g.Genere)).ToList();
    }
}