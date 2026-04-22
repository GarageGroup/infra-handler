using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GarageGroup.Infra;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Builder;

partial class HandlerApplicationBuilder
{
    public static TApplicationBuilder UseEndpoint<TApplicationBuilder, TIn, TOut>(
        this TApplicationBuilder app,
        Func<IServiceProvider, IHandler<TIn, TOut>> handlerResolver,
        string verb,
        [AllowNull, StringSyntax("Route")] string template = DefaultRoute)
        where TApplicationBuilder : IApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(handlerResolver);

        return app.InternalUseEndpoint(handlerResolver, verb, template);
    }

    internal static TApplicationBuilder InternalUseEndpoint<TApplicationBuilder, TIn, TOut>(
        this TApplicationBuilder app,
        Func<IServiceProvider, IHandler<TIn, TOut>> handlerResolver,
        string verb,
        string? template)
        where TApplicationBuilder : IApplicationBuilder
    {
        IRouteBuilder routeBuilder = new RouteBuilder(app);

        if (string.IsNullOrEmpty(template))
        {
            template = DefaultRoute;
        }

        if (string.IsNullOrEmpty(verb))
        {
            routeBuilder = routeBuilder.MapGet(template, InnerInvokeAsync);
        }
        else
        {
            routeBuilder = routeBuilder.MapVerb(verb, template, InnerInvokeAsync);
        }

        _ = app.UseRouter(routeBuilder.Build());

        return app;

        Task InnerInvokeAsync(HttpContext context)
        {
            if (context.RequestAborted.IsCancellationRequested)
            {
                return Task.FromCanceled(context.RequestAborted);
            }

            return InvokeAsync(context, handlerResolver.Invoke(context.RequestServices));
        }
    }

    private static async Task<Unit> InvokeAsync<TIn, TOut>(HttpContext context, IHandler<TIn, TOut> handler)
    {
        var json = await context.Request.Body.ReadAsStringAsync(context.RequestAborted).ConfigureAwait(false);
        var result = await json.DeserializeOrFailure<TIn>().ForwardValueAsync(InnerHandleAsync).ConfigureAwait(false);

        return await result.FoldValueAsync(context.WriteSuccessAsync, context.WriteFailureAsync).ConfigureAwait(false);

        ValueTask<Result<TOut, Failure<HandlerFailureCode>>> InnerHandleAsync(TIn? input)
            =>
            handler.HandleAsync(input, context.RequestAborted);
    }

    private static async ValueTask<Unit> WriteSuccessAsync<TOut>(this HttpContext context, TOut success)
    {
        if (success is Unit || success is null)
        {
            context.Response.StatusCode = SuccessEmptyStatusCode;
            return default;
        }

        context.Response.StatusCode = SuccessStatusCode;

        if (success is string text)
        {
            await context.Response.WriteAsync(text, context.RequestAborted).ConfigureAwait(false);
            return default;
        }

        await context.Response.WriteAsJsonAsync(success, SerializerOptions, context.RequestAborted).ConfigureAwait(false);
        return default;
    }

    private static async ValueTask<Unit> WriteFailureAsync(this HttpContext context, Failure<HandlerFailureCode> failure)
    {
        var logger = context.GetLogger();
        if (failure.FailureCode is HandlerFailureCode.Persistent)
        {
            logger?.LogError(failure.SourceException, "An unexpected persistent handler HTTP error occured: {error}", failure.FailureMessage);
            context.Response.StatusCode = PersistentFailureStatusCode;
        }
        else
        {
            logger?.LogError(failure.SourceException, "An unexpected transient handler HTTP error occured: {error}", failure.FailureMessage);
            context.Response.StatusCode = TransientFailureStatusCode;
        }

        if (string.IsNullOrEmpty(failure.FailureMessage))
        {
            return default;
        }

        await context.Response.WriteAsync(failure.FailureMessage, context.RequestAborted).ConfigureAwait(false);
        return default;
    }
}