using Microsoft.CodeAnalysis;

namespace GarageGroup.Infra;

[Generator]
internal sealed class HandlerApplicationSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var rootType in context.GetRootTypes())
        {
            var constructorSourceCode = rootType.BuildConstructorSourceCode();
            context.AddSource($"{rootType.TypeName}.g.cs", constructorSourceCode);

            foreach (var resolverType in rootType.ResolverTypes)
            {
                var endpointSourceCode = rootType.BuildEndpointSourceCode(resolverType);
                context.AddSource($"{rootType.TypeName}.{resolverType.ResolverMethodName}.g.cs", endpointSourceCode);
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}