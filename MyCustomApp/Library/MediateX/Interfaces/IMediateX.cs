namespace MediateX.Interfaces;

public interface IMediateX
{
    Task<TResult> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    Task<TResult> Query<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}