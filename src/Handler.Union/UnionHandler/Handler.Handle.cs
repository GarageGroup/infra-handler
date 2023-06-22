using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

partial class UnionHandler<T>
{
    public ValueTask<Result<Unit, Failure<HandlerFailureCode>>> HandleAsync(T? input, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<Unit, Failure<HandlerFailureCode>>>(cancellationToken);
        }

        if (innerHandlers.Length is 0)
        {
            return new(Result.Success<Unit>(default));
        }

        return InnerHandleAsync(input, cancellationToken);
    }

    private async ValueTask<Result<Unit, Failure<HandlerFailureCode>>> InnerHandleAsync(T? input, CancellationToken cancellationToken)
    {
        foreach (var innerHandler in innerHandlers)
        {
            var result = await innerHandler.HandleAsync(input, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return result.FailureOrThrow();
            }
        }

        return Result.Success<Unit>(default);
    }
}