using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra;

public readonly record struct HandlerFailure
{
    public static HandlerFailure Retry([AllowNull] string failureMessage)
        =>
        new(HandlerFailureAction.Retry, failureMessage);

    public static HandlerFailure Remove([AllowNull] string failureMessage)
        =>
        new(HandlerFailureAction.Remove, failureMessage);

    private readonly string? failureMessage;

    public HandlerFailure(HandlerFailureAction failureAction, [AllowNull] string failureMessage)
    {
        FailureAction = failureAction;
        this.failureMessage = string.IsNullOrEmpty(failureMessage) ? null : failureMessage;
    }

    public HandlerFailureAction FailureAction { get; }

    public string FailureMessage
        =>
        failureMessage ?? string.Empty;
}