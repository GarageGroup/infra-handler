using System;

namespace GGroupp.Infra;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class HandlerDataJsonAttribute : Attribute
{
    public HandlerDataJsonAttribute(string rootPath)
        =>
        RootPath = rootPath ?? string.Empty;

    public string RootPath { get; }
}