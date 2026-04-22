using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class SourceGeneratorExtensions
{
    internal static IReadOnlyCollection<RootTypeMetadata> GetRootTypes(this Compilation compilation, CancellationToken cancellationToken)
    {
        var visitor = new ExportedTypesCollector(cancellationToken);
        visitor.VisitNamespace(compilation.GlobalNamespace);

        return [..visitor.GetNonPrivateTypes().Select(GetRootType).NotNull()];
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

        var handlerType = methodSymbol.ReturnType.GetResolvedHandlerType(methodSymbol);

        if (IsHandlerType(handlerType) is false && handlerType.AllInterfaces.Any(IsHandlerType) is not true)
        {
            throw methodSymbol.CreateInvalidMethodException($"must resolve a type that implements {DefaultNamespace}.IHandler<TIn, TOut>");
        }

        return new(
            resolverMethodName: methodSymbol.Name,
            endpoint: new(
                method: extensionAttribute.ConstructorArguments.Length > 0 ? extensionAttribute.ConstructorArguments[0].Value?.ToString() : null,
                route: extensionAttribute.ConstructorArguments.Length > 1 ? extensionAttribute.ConstructorArguments[1].Value?.ToString() : null));

        static bool IsHandlerApplicationExtensionAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(DefaultNamespace, "HandlerApplicationExtensionAttribute") is true;

        static bool IsHandlerType(INamedTypeSymbol? typeSymbol)
            =>
            typeSymbol?.IsType(DefaultNamespace, "IHandler") is true && typeSymbol.TypeParameters.Length is 2;
    }

    private static INamedTypeSymbol GetResolvedHandlerType(this ITypeSymbol dependencyType, IMethodSymbol methodSymbol)
    {
        if (dependencyType is not INamedTypeSymbol namedDependencyType)
        {
            throw methodSymbol.CreateInvalidMethodException(
                "return type must be a named type with public instance Resolve(System.IServiceProvider) method");
        }

        var resolveMethod = EnumerateResolveMethods(namedDependencyType).FirstOrDefault(IsResolveContractMatch)
         ?? throw methodSymbol.CreateInvalidMethodException(
                "return type must contain a public instance Resolve(System.IServiceProvider) method without generic arguments");

        if (resolveMethod.ReturnType is not INamedTypeSymbol handlerType)
        {
            throw methodSymbol.CreateInvalidMethodException(
                "Resolve(System.IServiceProvider) must return a named type");
        }

        return handlerType;

        static IEnumerable<IMethodSymbol> EnumerateResolveMethods(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind is TypeKind.Interface)
            {
                foreach (var methodSymbol in typeSymbol.GetMembers("Resolve").OfType<IMethodSymbol>())
                {
                    yield return methodSymbol;
                }

                foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
                {
                    foreach (var methodSymbol in interfaceSymbol.GetMembers("Resolve").OfType<IMethodSymbol>())
                    {
                        yield return methodSymbol;
                    }
                }

                yield break;
            }

            for (var currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
            {
                foreach (var methodSymbol in currentType.GetMembers("Resolve").OfType<IMethodSymbol>())
                {
                    yield return methodSymbol;
                }
            }
        }

        static bool IsResolveContractMatch(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind is not MethodKind.Ordinary)
            {
                return false;
            }

            if (methodSymbol.IsStatic)
            {
                return false;
            }

            if (methodSymbol.DeclaredAccessibility is not Accessibility.Public)
            {
                return false;
            }

            if (methodSymbol.TypeParameters.Any())
            {
                return false;
            }

            if (methodSymbol.Parameters.Length is not 1)
            {
                return false;
            }

            var parameterSymbol = methodSymbol.Parameters[0];
            if (parameterSymbol.RefKind is not RefKind.None)
            {
                return false;
            }

            if (parameterSymbol.Type.IsType("System", "IServiceProvider") is false)
            {
                return false;
            }

            return methodSymbol.ReturnsVoid is false;
        }
    }
}
