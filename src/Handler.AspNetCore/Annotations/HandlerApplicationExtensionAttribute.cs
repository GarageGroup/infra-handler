using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HandlerApplicationExtensionAttribute : Attribute
{
    private const string DefaultRoute = "/";

    public HandlerApplicationExtensionAttribute(string method = HttpMethodName.Get, string route = DefaultRoute)
    {
        Method = string.IsNullOrEmpty(method) ? HttpMethodName.Get : method;
        Route = string.IsNullOrEmpty(route) ? DefaultRoute : route;
    }

    public string Method { get; }

    public string Route { get; }
}