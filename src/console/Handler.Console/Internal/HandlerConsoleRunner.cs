using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra;

public sealed class HandlerConsoleRunner : IHandlerConsoleRunner
{
    private readonly Func<IServiceProvider, IHandler<Unit, Unit>> handlerResolver;

    private readonly IConfiguration configuration;

    private readonly Action<IServiceCollection>? configureServices;

    private readonly Action<ILoggingBuilder>? configureLogger;

    internal HandlerConsoleRunner(Func<IServiceProvider, IHandler<Unit, Unit>> handlerResolver, [AllowNull] string[] args)
    {
        this.handlerResolver = handlerResolver;
        configuration = BuildConfiguration(args ?? []);
    }

    private HandlerConsoleRunner(
        Func<IServiceProvider, IHandler<Unit, Unit>> handlerResolver,
        IConfiguration configuration,
        Action<IServiceCollection>? configureServices,
        Action<ILoggingBuilder>? configureLogger)
    {
        this.handlerResolver = handlerResolver;
        this.configuration = configuration;
        this.configureServices = configureServices;
        this.configureLogger = configureLogger;
    }

    public IHandlerConsoleRunner Configure(
        Action<IServiceCollection> configureServices,
        Action<ILoggingBuilder>? configureLogger = null)
        =>
        new HandlerConsoleRunner(handlerResolver, configuration, configureServices, configureLogger);

    public async Task RunAsync()
    {
        using var serviceProvider = CreateServiceProvider();
        using var cancellationTokenSource = GetCancellationTokenSource();

        var handler = handlerResolver.Invoke(serviceProvider);
        var result = await InnerInvokeAsync(handler, cancellationTokenSource.Token);

        _ = result.Fold(Unit.From, InnerLogFailre);

        Unit InnerLogFailre(Failure<HandlerFailureCode> failure)
        {
            if (failure.FailureCode is not HandlerFailureCode.Persistent)
            {
                throw new InvalidOperationException($"An unexpected persistent error occured: {failure.FailureMessage}", failure.SourceException);
            }

            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<HandlerConsoleRunner>();
            logger.LogError(failure.SourceException, "An unexpected transient failure occured: {failureMessage}", failure.FailureMessage);

            return default;
        }
    }

    private static async Task<Result<Unit, Failure<HandlerFailureCode>>> InnerInvokeAsync(
        IHandler<Unit, Unit> handler, CancellationToken cancellationToken)
    {
        try
        {
            return await handler.HandleAsync(default, cancellationToken);
        }
        finally
        {
            if (handler is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection()
            .AddLogging(InnerConfigureLogger)
            .AddSingleton(configuration)
            .AddSocketsHttpHandlerProviderAsSingleton()
            .AddTokenCredentialStandardAsSingleton();

        configureServices?.Invoke(services);

        return services.BuildServiceProvider();

        void InnerConfigureLogger(ILoggingBuilder builder)
        {
            builder = builder.AddConsole();
            configureLogger?.Invoke(builder);
        }
    }

    private CancellationTokenSource GetCancellationTokenSource()
    {
        var timeout = configuration.GetValue<TimeSpan?>("MaxTimeout");
        return timeout is null ? new() : new(timeout.Value);
    }

    private static IConfiguration BuildConfiguration(string[] args)
        =>
        new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();
}