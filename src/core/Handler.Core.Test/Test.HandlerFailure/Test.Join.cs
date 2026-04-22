using System;
using Xunit;

namespace GarageGroup.Infra.Handler.Core.Test;

partial class HandlerFailureTest
{
    [Fact]
    public static void Join_SourceAndFailureAreTransient_ReturnsTransient()
    {
        var source = Failure.Create(HandlerFailureCode.Transient, "source");
        var failure = Failure.Create(HandlerFailureCode.Transient, "failure");

        var result = source.Join(failure, "joined");

        Assert.Equal(HandlerFailureCode.Transient, result.FailureCode);
        Assert.Equal("joined", result.FailureMessage);
    }

    [Theory]
    [InlineData(HandlerFailureCode.Persistent, HandlerFailureCode.Transient)]
    [InlineData(HandlerFailureCode.Transient, HandlerFailureCode.Persistent)]
    [InlineData(HandlerFailureCode.Persistent, HandlerFailureCode.Persistent)]
    public static void Join_AnyFailureIsPersistent_ReturnsPersistent(
        HandlerFailureCode sourceCode, HandlerFailureCode failureCode)
    {
        var source = Failure.Create(sourceCode, "source");
        var failure = Failure.Create(failureCode, "failure");

        var result = source.Join(failure, "joined");

        Assert.Equal(HandlerFailureCode.Persistent, result.FailureCode);
    }

    [Fact]
    public static void Join_BuildsAggregateExceptionFromBothFailures()
    {
        var sourceInner = new Exception("source inner");
        var failureInner = new Exception("failure inner");

        var source = Failure.Create(HandlerFailureCode.Transient, "source message", sourceInner);
        var failure = Failure.Create(HandlerFailureCode.Persistent, "failure message", failureInner);

        var result = source.Join(failure, "joined");
        var aggregate = Assert.IsType<AggregateException>(result.SourceException);

        Assert.Collection(
            aggregate.InnerExceptions,
            sourceException =>
            {
                var invalid = Assert.IsType<InvalidOperationException>(sourceException);
                Assert.Equal("source message", invalid.Message);
                Assert.Same(sourceInner, invalid.InnerException);
            },
            failureException =>
            {
                var invalid = Assert.IsType<InvalidOperationException>(failureException);
                Assert.Equal("failure message", invalid.Message);
                Assert.Same(failureInner, invalid.InnerException);
            });
    }
}