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
    public static Task<Unit> RunConsoleAsync<THandler, TIn>(
        this Dependency<THandler> dependency, [AllowNull] string inputSection = null, [AllowNull] string[] args = null)
        where THandler : IHandler<TIn, Unit>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.InnerRunConsoleAsync<THandler, TIn>(inputSection, args);
    }

    public static Task<Unit> RunConsoleAsync<TIn>(
        this Dependency<IHandler<TIn, Unit>> dependency, [AllowNull] string inputSection = null, [AllowNull] string[] args = null)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.InnerRunConsoleAsync<IHandler<TIn, Unit>, TIn>(inputSection, args);
    }

    private static async Task<Unit> InnerRunConsoleAsync<THandler, TIn>(
        this Dependency<THandler> dependency, [AllowNull] string inputSection, [AllowNull] string[] args)
        where THandler : IHandler<TIn, Unit>
    {
        var configuration = BuildConfiguration(args ?? Array.Empty<string>());

        using var serviceProvider = configuration.CreateServiceProvider();
        using var cancellationTokenSource = configuration.GetCancellationTokenSource();

        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("HandlerConsoleRunner");

        var input = configuration.ReadInput<TIn>(inputSection ?? string.Empty);
        var result = await dependency.Resolve(serviceProvider).HandleAsync(input, cancellationTokenSource.Token);

        return result.Fold(Unit.From, logger.LogFailure);
    }

    private static TIn? ReadInput<TIn>(this IConfiguration configuration, string sectionName)
    {
        if (typeof(TIn) == typeof(Unit))
        {
            return default;
        }

        return configuration.GetRequiredSection(sectionName).Get<TIn>();
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
        .AddTokenCredentialStandardAsSingleton()
        .BuildServiceProvider();

    private static IConfiguration BuildConfiguration(string[] args)
        =>
        new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();
}