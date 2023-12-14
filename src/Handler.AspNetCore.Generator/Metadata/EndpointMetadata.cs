namespace GarageGroup.Infra;

internal sealed record class EndpointMetadata
{
    private const string DefaultMethod = "GET";

    private const string DefaultRoute = "/";

    public EndpointMetadata(string? method, string? route)
    {
        Method = string.IsNullOrEmpty(method) ? DefaultMethod : method!;
        Route = string.IsNullOrEmpty(route) ? DefaultRoute : route!;
    }

    public string Method { get; }

    public string Route { get; }
}