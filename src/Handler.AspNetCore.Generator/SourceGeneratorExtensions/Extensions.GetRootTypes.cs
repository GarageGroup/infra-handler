using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static IReadOnlyCollection<RootTypeMetadata> GetRootTypes(this GeneratorExecutionContext context)
    {
        var visitor = new ExportedTypesCollector(context.CancellationToken);
        visitor.VisitNamespace(context.Compilation.GlobalNamespace);

        return visitor.GetNonPrivateTypes().Select(GetRootType).NotNull().ToArray();
    }

    private static RootTypeMetadata? GetRootType(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeArguments.Any())
        {
            return null;
        }

        var resolverTypes = typeSymbol.GetMembers().OfType<IMethodSymbol>().Select(GetResolver).NotNull().ToArray();
        if (resolverTypes.Any() is false)
        {
            return null;
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "HandlerExtensions",
            providerType: typeSymbol.GetDisplayedData(),
            resolverTypes: resolverTypes);
    }

    private static ResolverMetadata? GetResolver(IMethodSymbol methodSymbol)
    {
        var extensionAttribute = methodSymbol.GetAttributes().FirstOrDefault(IsHandlerApplicationExtensionAttribute);
        if (extensionAttribute is null)
        {
            return null;
        }

        if (methodSymbol.IsStatic is false)
        {
            throw methodSymbol.CreateInvalidMethodException("must be static");
        }

        if (methodSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            throw methodSymbol.CreateInvalidMethodException("must be public or internal");
        }

        if (methodSymbol.TypeParameters.Any())
        {
            throw methodSymbol.CreateInvalidMethodException("must have no generic arguments");
        }

        if (methodSymbol.Parameters.Any())
        {
            throw methodSymbol.CreateInvalidMethodException("must have no parameters");
        }

        var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
        if (returnType?.IsType("PrimeFuncPack", "Dependency") is not true || returnType?.TypeArguments.Length is not 1)
        {
            throw methodSymbol.CreateInvalidMethodException("return type must be PrimeFuncPack.Dependency<THandler>");
        }

        var handlerType = returnType.TypeArguments[0] as INamedTypeSymbol;
        if (IsHandlerType(handlerType) is false && handlerType?.AllInterfaces.Any(IsHandlerType) is not true)
        {
            throw methodSymbol.CreateInvalidMethodException($"must resolve a type that implements {DefaultNamespace}.IHandler<TIn, TOut>");
        }

        return new(
            resolverMethodName: methodSymbol.Name,
            endpoint: new(
                method: extensionAttribute.GetAttributeValue(0)?.ToString(),
                route: extensionAttribute.GetAttributeValue(1)?.ToString()));

        static bool IsHandlerApplicationExtensionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "HandlerApplicationExtensionAttribute") is true;

        static bool IsHandlerType(INamedTypeSymbol? typeSymbol)
            =>
            typeSymbol?.IsType(DefaultNamespace, "IHandler") is true && typeSymbol.TypeParameters.Length is 2;
    }
}