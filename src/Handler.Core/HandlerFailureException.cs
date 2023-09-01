using System;

namespace GarageGroup.Infra;

public sealed class HandlerFailureException : InvalidOperationException
{
    public HandlerFailureException(Failure<HandlerFailureCode> failure)
        : base(failure.FailureMessage, failure.SourceException)
        =>
        Failure = failure;

    public Failure<HandlerFailureCode> Failure { get; }
}