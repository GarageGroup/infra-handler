using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GarageGroup.Infra;

internal sealed class AdapterHandler<TIn> : IHandler<Unit, Unit>
{
    private readonly IHandler<TIn, Unit> innerHandler;

    private readonly IConfiguration configuration;

    private readonly string sectionName;

    internal AdapterHandler(IHandler<TIn, Unit> innerHandler, IConfiguration configuration, [AllowNull] string sectionName)
    {
        this.innerHandler = innerHandler;
        this.configuration = configuration;
        this.sectionName = sectionName ?? string.Empty;
    }

    public ValueTask<Result<Unit, Failure<HandlerFailureCode>>> HandleAsync(
        Unit _, CancellationToken cancellationToken)
    {
        return ReadInput().ForwardValueAsync(InnerHandleAsync);

        ValueTask<Result<Unit, Failure<HandlerFailureCode>>> InnerHandleAsync(TIn? @in)
            =>
            innerHandler.HandleAsync(@in, cancellationToken);
    }

    private Result<TIn?, Failure<HandlerFailureCode>> ReadInput()
    {
        try
        {
            return configuration.GetRequiredSection(sectionName).Get<TIn>();
        }
        catch (Exception ex)
        {
            return ex.ToFailure(
                HandlerFailureCode.Persistent,
                $"Input of type '{typeof(TIn).FullName}' can't be readed from section '{sectionName}'");
        }
    }
}