using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra;

public interface IHandler<THandlerData>
{
    ValueTask<Result<Unit, HandlerFailure>> HandleAsync(THandlerData? handlerData, CancellationToken cancellationToken);
}