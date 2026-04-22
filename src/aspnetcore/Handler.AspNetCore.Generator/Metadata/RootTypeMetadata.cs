using System.Collections.Generic;
using PrimeFuncPack;

namespace GarageGroup.Infra;

internal sealed record class RootTypeMetadata
{
    public RootTypeMetadata(
        string @namespace,
        string typeName,
        DisplayedTypeData providerType,
        IReadOnlyList<ResolverMetadata> resolverTypes)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
        ProviderType = providerType;
        ResolverTypes = resolverTypes ?? [];
    }

    public string Namespace { get; }

    public string TypeName { get; }

    public DisplayedTypeData ProviderType { get; }

    public IReadOnlyList<ResolverMetadata> ResolverTypes { get; }
}