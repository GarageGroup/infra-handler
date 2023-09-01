using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class ConsoleDependencyExtensions
{
    public static async Task<Unit> RunConsoleAsync<THandler>(this Dependency<THandler> dependency, [AllowNull] string[] args = null)
        where THandler : IHandler<Unit, Unit>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        var configuration = BuildConfiguration(args ?? Array.Empty<string>());

        using var serviceProvider = configuration.CreateServiceProvider();
        using var cancellationTokenSource = configuration.GetCancellationTokenSource();

        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("HandlerConsoleRunner");
        var result = await dependency.Resolve(serviceProvider).HandleAsync(default, cancellationTokenSource.Token);

        return result.Fold(Unit.From, logger.LogFailure);
    }

    private static Unit LogFailure(this ILogger logger, Failure<HandlerFailureCode> failure)
    {
        if (failure.FailureCode is not HandlerFailureCode.Persistent)
        {
            throw new InvalidOperationException($"An unexpected error has occured: {failure.FailureMessage}", failure.SourceException);
        }

        logger.LogError(failure.SourceException, "An unexpected failure has occured: {failureMessage}", failure.FailureMessage);
        return default;
    }

    private static CancellationTokenSource GetCancellationTokenSource(this IConfiguration configuration)
    {
        var timeout = configuration.GetValue<TimeSpan?>("MaxTimeout");
        return timeout is null ? new() : new(timeout.Value);
    }

    private static ServiceProvider CreateServiceProvider(this IConfiguration configuration)
        =>
        new ServiceCollection()
        .AddLogging(
            static builder => builder.AddConsole())
        .AddSingleton(
            configuration)
        .AddSocketsHttpHandlerProviderAsSingleton()
        .BuildServiceProvider();

    private static IConfiguration BuildConfiguration(string[] args)
        =>
        new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();
}