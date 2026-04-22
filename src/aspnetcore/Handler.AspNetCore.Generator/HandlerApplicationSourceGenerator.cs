using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GarageGroup.Infra;

[Generator(LanguageNames.CSharp)]
internal sealed class HandlerApplicationSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.CompilationProvider.Select(SourceGeneratorExtensions.GetRootTypes);
        context.RegisterSourceOutput(provider, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, IReadOnlyCollection<RootTypeMetadata> rootTypes)
    {
        foreach (var rootType in rootTypes)
        {
            var constructorSourceCode = rootType.BuildConstructorSourceCode();
            context.AddSource($"{rootType.TypeName}.g.cs", SourceText.From(constructorSourceCode, Encoding.UTF8));

            foreach (var resolverType in rootType.ResolverTypes)
            {
                var endpointSourceCode = rootType.BuildEndpointSourceCode(resolverType);

                context.AddSource(
                    $"{rootType.TypeName}.{resolverType.ResolverMethodName}.g.cs", SourceText.From(endpointSourceCode, Encoding.UTF8));
            }
        }
    }
}