using System.Text;

namespace GarageGroup.Infra;

partial class HandlerApplicationBuilder
{
    internal static string BuildEndpointSourceCode(this RootTypeMetadata rootType, ResolverMetadata resolver)
        =>
        new SourceBuilder(
            rootType.Namespace)
        .AddUsing(
            "Microsoft.AspNetCore.Builder")
        .AppendCodeLine(
            $"partial class {rootType.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"internal static TBuilder {resolver.ResolverMethodName}<TBuilder>(this TBuilder builder) where TBuilder : IApplicationBuilder")
        .BeginLambda()
        .AppendCodeLine(
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