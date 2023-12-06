using System;

namespace GarageGroup.Infra;

[Obsolete("This class is obsolete. Use Failure<HandlerFailureCode>.Exception instead")]
public sealed class HandlerFailureException : InvalidOperationException
{
    public HandlerFailureException(Failure<HandlerFailureCode> failure)
        : base(failure.FailureMessage, failure.SourceException)
        =>
        Failure = failure;

    public Failure<HandlerFailureCode> Failure { get; }
}