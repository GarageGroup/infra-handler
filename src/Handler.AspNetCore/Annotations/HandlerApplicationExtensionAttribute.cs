using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HandlerApplicationExtensionAttribute(
    string method = HttpMethodName.Get,
    string route = HandlerApplicationExtensionAttribute.DefaultRoute)
    : Attribute
{
    private const string DefaultRoute = "/";

    public string Method { get; } = string.IsNullOrEmpty(method) ? HttpMethodName.Get : method;

    public string Route { get; } = string.IsNullOrEmpty(route) ? DefaultRoute : route;
}