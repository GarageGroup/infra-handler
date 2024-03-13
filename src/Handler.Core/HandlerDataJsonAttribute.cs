using System;

namespace GarageGroup.Infra;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class HandlerDataJsonAttribute(string rootPath) : Attribute
{
    public string RootPath { get; } = string.IsNullOrEmpty(rootPath) ? string.Empty : rootPath;
}