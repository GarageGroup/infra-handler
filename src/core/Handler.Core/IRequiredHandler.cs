using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

public interface IRequiredHandler<THandlerIn, THandlerOut> : IHandler<THandlerIn, THandlerOut>
    where THandlerIn : class
{
    ValueTask<Result<THandlerOut, Failure<HandlerFailureCode>>> HandleRequiredAsync(
        THandlerIn input, CancellationToken cancellationToken);

    ValueTask<Result<THandlerOut, Failure<HandlerFailureCode>>> IHandler<THandlerIn, THandlerOut>.HandleAsync(
        THandlerIn? input, CancellationToken cancellationToken)
    {
        if (input is null)
        {
            return new(
                result: Failure.Create(HandlerFailureCode.Persistent, "Input must be specified."));
        }

        return HandleRequiredAsync(input, cancellationToken);
    }
}