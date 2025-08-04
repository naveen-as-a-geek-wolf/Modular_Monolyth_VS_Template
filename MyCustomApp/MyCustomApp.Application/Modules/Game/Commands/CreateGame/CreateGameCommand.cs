using LanguageExt.Common;
using MediateX.Interfaces;
namespace MyCustomApp.Application.Modules.Game.Commands.CreateGame;

public record CreateGameCommand(string Name, string Genre) : ICommand<Result<GameCreatedResult>>;

public record GameCreatedResult(int Id, string Name, string Genre);