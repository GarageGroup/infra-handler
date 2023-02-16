using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra;

partial class UnionHandler<T>
{
    public ValueTask<Result<Unit, HandlerFailure>> HandleAsync(T? handlerData, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<Unit, HandlerFailure>>(cancellationToken);
        }

        if (innerHandlers.Length is 0)
        {
            return new(Result.Success<Unit>(default));
        }

        return InnerHandleAsync(handlerData, cancellationToken);
    }

    private async ValueTask<Result<Unit, HandlerFailure>> InnerHandleAsync(T? handlerData, CancellationToken cancellationToken)
    {
        foreach (var innerHandler in innerHandlers)
        {
            var result = await innerHandler.HandleAsync(handlerData, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return result.FailureOrThrow();
            }
        }

        return Result.Success<Unit>(default);
    }
}