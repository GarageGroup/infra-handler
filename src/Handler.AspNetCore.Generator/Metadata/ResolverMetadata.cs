namespace GarageGroup.Infra;

internal sealed record class ResolverMetadata
{
    public ResolverMetadata(string resolverMethodName, EndpointMetadata endpoint)
    {
        ResolverMethodName = resolverMethodName ?? string.Empty;
        Endpoint = endpoint;
    }

    public string ResolverMethodName { get; }

    public EndpointMetadata Endpoint { get; }
}