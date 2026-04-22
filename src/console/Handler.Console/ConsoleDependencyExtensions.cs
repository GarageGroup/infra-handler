using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class ConsoleDependencyExtensions
{
    public static HandlerConsoleRunner UseConsoleRunner<THandler>(
        this Dependency<IHandler<Unit, Unit>> dependency, [AllowNull] string[] args = null)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return new(dependency.Resolve, args);
    }

    public static HandlerConsoleRunner UseConsoleRunner<THandler>(
        this Dependency<THandler> dependency, [AllowNull] string[] args = null)
        where THandler : IHandler<Unit, Unit>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return new(InnerResolve, args);

        IHandler<Unit, Unit> InnerResolve(IServiceProvider serviceProvider)
            =>
            dependency.Resolve(serviceProvider);
    }

    public static HandlerConsoleRunner UseConsoleRunner<TIn>(
        this Dependency<IHandler<TIn, Unit>> dependency,
        string inputSection,
        [AllowNull] string[] args = null)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return new(InnerResolve, args);

        IHandler<Unit, Unit> InnerResolve(IServiceProvider serviceProvider)
            =>
            new AdapterHandler<TIn>(
                innerHandler: dependency.Resolve(serviceProvider),
                configuration: serviceProvider.GetRequiredService<IConfiguration>(),
                sectionName: inputSection);
    }
}