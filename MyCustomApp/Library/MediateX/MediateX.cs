using MediateX.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MediateX;

public class MediateX : IMediateX
{
    private readonly IServiceProvider _serviceProvider;

    public MediateX(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResult> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var handler = GetHandler<ICommandHandler<TCommand, TResult>>();
        return await handler.Handle(command, cancellationToken);
    }

    public async Task<TResult> Query<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        IQueryHandler<TQuery, TResult> handler = GetHandler<IQueryHandler<TQuery, TResult>>();
        return await handler.Handle(query, cancellationToken);
    }

    private THandler GetHandler<THandler>()
        where THandler : notnull
    {
        return _serviceProvider.GetRequiredService<THandler>();
    }
}