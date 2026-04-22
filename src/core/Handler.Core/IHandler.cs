using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

public interface IHandler<THandlerIn, THandlerOut>
{
    ValueTask<Result<THandlerOut, Failure<HandlerFailureCode>>> HandleAsync(THandlerIn? input, CancellationToken cancellationToken);
}