using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class HandlerApplicationDependency
{
    public static TApplicationBuilder MapEndpoint<TApplicationBuilder, TIn, TOut>(
        this Dependency<IHandler<TIn, TOut>> dependency,
        TApplicationBuilder applicationBuilder,
        string verb,
        [AllowNull, StringSyntax("Route")] string template)
        where TApplicationBuilder : IApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(applicationBuilder);

        return applicationBuilder.InternalUseEndpoint(dependency.Resolve, verb, template);
    }
}