using MyCustomApp.API.Endpoints.Interface;
using MyCustomApp.API.Extensions;
using MyCustomApp.Application.Modules.Game.Commands.CreateGame;
using MyCustomApp.Application.Modules.Game.Queries;
using LanguageExt.Common;
using MediateX.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MyCustomApp.API.Endpoints.Modules.Game
{
    public class GamesEndPoint : IEndpoint
    {
        public void MapEndpoints(IEndpointRouteBuilder endpoints)
        {

            endpoints.MapPost("api/Games", async ([FromBody] CreateGameCommand command,
                                               IMediateX mediatex,
                                               CancellationToken cancellationToken) =>
            {
                return (await mediatex.Send<CreateGameCommand, Result<GameCreatedResult>>(command, cancellationToken)).ToResponse(x => x);
            })
                .WithName("Create Games")
                            .WithSummary("Endpoint to create Custom games with Name and genere")
                            .WithDescription(@"
                              Creates a new custom game by accepting essential game details such as name and genre.

                                  **Request Body:**
                                  - `name` (string): The name of the game. Must be unique.
                                  - `genre` (string): The genre or category of the game (e.g., strategy, action, puzzle).
                                  
                                  **Responses:**
                                  - **200 OK:** Returns a result containing details of the created game.
                                  - **400 Bad Request:** Returned when input data is invalid or missing required fields.
                                  - **500 Internal Server Error:** Returned when an unexpected server-side error occurs.
                                                                    
                                  **Example Request Body:**
                                  ```json
                                  {
                                    ""name"": ""Galaxy Conquerors"",
                                    ""genre"": ""Strategy""
                                  }
                                  ```
                            ")
                            .Produces<Result<GameCreatedResult>>(200)
                            .Produces(400)
                            .Produces(500);

            endpoints.MapGet("api/Games", async (
               [AsParameters] GetAllGamesQuery query,
                IMediateX mediatex,
                CancellationToken cancellationToken) =>
            {
                return (await mediatex.Query<GetAllGamesQuery, Result<List<GameCreatedResult>>>(query, cancellationToken)).ToResponse(x => x);
            })
            .WithName("Get All Games")
            .WithSummary("Endpoint to get all created games")
            .WithDescription(@"
                Fetches all created games.
                
                **Responses:**
                - **200 OK:** Returns a list of created games.
                - **500 Internal Server Error:** Returned when an unexpected server-side error occurs.
            ")
            .Produces<Result<List<GameCreatedResult>>>(200)
            .Produces(400)
            .Produces(500)
            ;
            


        }
    }
}
