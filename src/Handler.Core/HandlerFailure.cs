using System;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra;

public static class HandlerFailure
{
    public static HandlerFailureCode ToPersistentFailureCode<TSourceFailureCode>(TSourceFailureCode _)
        where TSourceFailureCode : struct
        =>
        HandlerFailureCode.Persistent;

    public static HandlerFailureCode ToTransientFailureCode<TSourceFailureCode>(TSourceFailureCode _)
        where TSourceFailureCode : struct
        =>
        HandlerFailureCode.Transient;

    public static Failure<HandlerFailureCode> Join(
        this Failure<HandlerFailureCode> source, Failure<HandlerFailureCode> failure, [AllowNull] string failureMessage)
    {
        var sourceException = new InvalidOperationException(source.FailureMessage, source.SourceException);
        var failureException = new InvalidOperationException(failure.FailureMessage, failure.SourceException);

        var failureCode = (source.FailureCode, failure.FailureCode) switch
        {
            (HandlerFailureCode.Transient, HandlerFailureCode.Transient) => HandlerFailureCode.Transient,
            _ => HandlerFailureCode.Persistent
        };

        return new(failureCode, failureMessage)
        {
            SourceException = new AggregateException(sourceException, failureException)
        };
    }

    public static HandlerFailureException ToException(this Failure<HandlerFailureCode> handlerFailure)
        =>
        new(handlerFailure);
}