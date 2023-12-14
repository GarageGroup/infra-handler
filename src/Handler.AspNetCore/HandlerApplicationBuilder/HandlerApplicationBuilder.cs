using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GarageGroup.Infra;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Builder;

public static partial class HandlerApplicationBuilder
{
    private const string DefaultRoute = "/";

    private const int SuccessEmptyStatusCode = 204;

    private const int SuccessStatusCode = 200;

    private const int PersistentFailureStatusCode = 400;

    private const int TransientFailureStatusCode = 500;

    private static readonly JsonSerializerOptions SerializerOptions;

    static HandlerApplicationBuilder()
        =>
        SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

    private static Result<T?, Failure<HandlerFailureCode>> DeserializeOrFailure<T>(this string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return default(T);
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }
        catch (Exception exception)
        {
            return exception.ToFailure(
                HandlerFailureCode.Persistent, "An unexpected error occured when the request body was being deserialized");
        }
    }

    private static async Task<string?> ReadAsStringAsync(this Stream stream, CancellationToken cancellationToken)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        return await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    private static ILogger? GetLogger(this HttpContext context)
        =>
        context.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("HttpHandlerFailure");
}