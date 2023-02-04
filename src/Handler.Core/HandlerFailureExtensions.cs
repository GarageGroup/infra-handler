using System;

namespace GGroupp.Infra;

public static class HandlerFailureExtensions
{
    public static HandlerFailure ToHandlerFailure<TFailureCode>(
        this Failure<TFailureCode> failure, Func<TFailureCode, HandlerFailureAction> mapFailureCode)
        where TFailureCode : struct
    {
        ArgumentNullException.ThrowIfNull(mapFailureCode);

        return new(
            failureAction: mapFailureCode.Invoke(failure.FailureCode),
            failureMessage: failure.FailureMessage);
    }
}