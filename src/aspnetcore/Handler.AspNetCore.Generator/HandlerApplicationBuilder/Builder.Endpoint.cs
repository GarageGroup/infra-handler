using System.Text;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class HandlerApplicationBuilder
{
    internal static string BuildEndpointSourceCode(this RootTypeMetadata rootType, ResolverMetadata resolver)
        =>
        new SourceBuilder(
            rootType.Namespace)
        .AddUsing(
            "Microsoft.AspNetCore.Builder")
        .AppendCodeLines(
            $"partial class {rootType.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLines(
            $"internal static TBuilder {resolver.ResolverMethodName}<TBuilder>(this TBuilder builder) where TBuilder : IApplicationBuilder")
        .BeginLambda()
        .AppendCodeLines(
            rootType.BuildEndpointCodeLine(resolver))
        .EndLambda()
        .EndCodeBlock()
        .Build();

    private static string BuildEndpointCodeLine(this RootTypeMetadata rootType, ResolverMetadata resolver)
        =>
        new StringBuilder(
            "builder.UseEndpoint(")
        .Append(
            rootType.ProviderType.DisplayedTypeName)
        .Append(
            '.')
        .Append(
            resolver.ResolverMethodName)
        .Append(
            "().Resolve, ")
        .Append(
            resolver.Endpoint.Method.AsStringSourceCodeOr())
        .Append(
            ", ")
        .Append(
            resolver.Endpoint.Route.AsStringSourceCodeOr())
        .Append(
            ");")
        .ToString();
}