using LanguageExt.Common;
using MediateX.Interfaces;
using MyCustomApp.Application.Contracts;
using MyCustomApp.Domain.Modules.Game;

namespace MyCustomApp.Application.Modules.Game.Commands.CreateGame;

public class CreateGameCommandHandler : ICommandHandler<CreateGameCommand, Result<GameCreatedResult>>
{
    private readonly IGameDbContext _dbContext;

    public CreateGameCommandHandler(IGameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GameCreatedResult>> Handle(CreateGameCommand command, CancellationToken cancellationToken = default)
    {
        var game = new Domain.Modules.Game.Game
        {
            Name = command.Name,
            Genere = command.Genre // Note: property is 'Genere' in entity
        };

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = new GameCreatedResult(
            Id: game.Id,
            Name: game.Name,
            Genre: game.Genere // Map back to expected result
        );

        return result;
    }
}