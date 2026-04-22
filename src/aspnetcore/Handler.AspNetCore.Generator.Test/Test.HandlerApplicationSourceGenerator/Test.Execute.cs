using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace GarageGroup.Infra.Handler.AspNetCore.Generator.Test;

partial class HandlerApplicationSourceGeneratorTest
{
    [Fact]
    public static void Execute_ValidResolver_GeneratesExtensionAndEndpointSources()
    {
        const string sourceCode =
            """
            using System;

            namespace GarageSome.Api.Users
            {
                using GarageGroup.Infra;

                public interface IUsersHandler : IHandler<int, string>;
            }

            namespace GarageSome.Api
            {
                using GarageGroup.Infra;
                using GarageSome.Api.Users;

                public static class ApiProvider
                {
                    [HandlerApplicationExtension("POST", "/users")]
                    public static Some.Test.CustomDependency<IUsersHandler> ResolveUsers()
                        =>
                        default!;
                }
            }

            namespace Some.Test
            {
                public sealed class CustomDependency<T>
                {
                    public T Resolve(IServiceProvider serviceProvider)
                        =>
                        default!;
                }
            }
            """;

        var result = RunGenerator(sourceCode);
        var generatedSources = result.Results.Single().GeneratedSources;

        Assert.Equal(2, generatedSources.Length);

        var constructor = generatedSources.Single(EqualsExtensions).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                namespace GarageSome.Api;

                internal static partial class ApiProviderHandlerExtensions
                {
                }
                """),
            NormalizeNewLines(constructor));

        var endpoint = generatedSources.Single(EqualsExtensionsResolveUsers).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using Microsoft.AspNetCore.Builder;

                namespace GarageSome.Api;

                partial class ApiProviderHandlerExtensions
                {
                    internal static TBuilder ResolveUsers<TBuilder>(this TBuilder builder) where TBuilder : IApplicationBuilder
                        =>
                        builder.UseEndpoint(ApiProvider.ResolveUsers().Resolve, "POST", "/users");
                }
                """),
            NormalizeNewLines(endpoint));

        static bool EqualsExtensions(GeneratedSourceResult source)
            =>
            source.HintName.Equals("ApiProviderHandlerExtensions.g.cs");

        static bool EqualsExtensionsResolveUsers(GeneratedSourceResult source)
            =>
            source.HintName.Equals("ApiProviderHandlerExtensions.ResolveUsers.g.cs");
    }

    [Fact]
    public static void Execute_ResolverIsNotStatic_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            using System;

            namespace GarageSome.Invalid
            {
                using GarageGroup.Infra;

                public interface IInvalidHandler : IHandler<int, string>
                {
                }

                public sealed class InvalidProvider
                {
                    [HandlerApplicationExtension]
                    public PrimeFuncPack.Dependency<IInvalidHandler> ResolveInvalid()
                        =>
                        default!;
                }
            }

            namespace PrimeFuncPack
            {
                public sealed class Dependency<T>
                {
                    public T Resolve(IServiceProvider serviceProvider)
                        =>
                        default!;
                }
            }
            """;

        var result = RunGenerator(sourceCode);
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("InvalidProvider.ResolveInvalid", exception.Message);
        Assert.Contains("must be static", exception.Message);
    }

    [Fact]
    public static void Execute_EmptyAttributeArguments_UsesDefaultEndpointValues()
    {
        const string sourceCode =
            """
            using System;

            namespace GarageSome.Default
            {
                using GarageGroup.Infra;

                public sealed record class DefaultIn(int Id);

                public static class DefaultProvider
                {
                    [HandlerApplicationExtension]
                    public static PrimeFuncPack.Dependency<IHandler<DefaultIn, Unit>> ResolveDefault()
                        =>
                        default!;
                }
            }

            namespace PrimeFuncPack
            {
                public sealed class Dependency<T>
                {
                    public T Resolve(IServiceProvider serviceProvider)
                        =>
                        default!;
                }
            }
            """;

        var result = RunGenerator(sourceCode);
        var generatedSources = result.Results.Single().GeneratedSources;

        var endpoint = generatedSources.Single(EqualsExtensions).SourceText.ToString();
        Assert.Equal(
            NormalizeNewLines(
                """
                // Auto-generated code by PrimeFuncPack
                #nullable enable

                using Microsoft.AspNetCore.Builder;

                namespace GarageSome.Default;

                partial class DefaultProviderHandlerExtensions
                {
                    internal static TBuilder ResolveDefault<TBuilder>(this TBuilder builder) where TBuilder : IApplicationBuilder
                        =>
                        builder.UseEndpoint(DefaultProvider.ResolveDefault().Resolve, "GET", "/");
                }
                """),
            NormalizeNewLines(endpoint));

        static bool EqualsExtensions(GeneratedSourceResult source)
            =>
            source.HintName.Equals("DefaultProviderHandlerExtensions.ResolveDefault.g.cs");
    }

    [Fact]
    public static void Execute_ReturnTypeWithoutResolveMethod_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            using System;

            namespace GarageSome.Invalid
            {
                using GarageGroup.Infra;

                public interface IInvalidHandler : IHandler<int, string>
                {
                }

                public static class InvalidProvider
                {
                    [HandlerApplicationExtension]
                    public static PrimeFuncPack.Dependency<IInvalidHandler> ResolveInvalid()
                        =>
                        default!;
                }
            }

            namespace PrimeFuncPack
            {
                public sealed class Dependency<T>
                {
                }
            }
            """;

        var result = RunGenerator(sourceCode);
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("InvalidProvider.ResolveInvalid", exception.Message);
        Assert.Contains("must contain a public instance Resolve(System.IServiceProvider) method without generic arguments", exception.Message);
    }

    [Fact]
    public static void Execute_ResolveReturnsNonNamedType_ThrowsInvalidOperationException()
    {
        const string sourceCode =
            """
            using System;

            namespace GarageSome.Invalid
            {
                using GarageGroup.Infra;

                public static class InvalidProvider
                {
                    [HandlerApplicationExtension]
                    public static PrimeFuncPack.Dependency ResolveInvalid()
                        =>
                        default!;
                }
            }

            namespace PrimeFuncPack
            {
                public sealed class Dependency
                {
                    public dynamic Resolve(IServiceProvider serviceProvider)
                        =>
                        default!;
                }
            }
            """;

        var result = RunGenerator(sourceCode);
        var exception = Assert.IsType<InvalidOperationException>(result.Results.Single().Exception);

        Assert.Contains("InvalidProvider.ResolveInvalid", exception.Message);
        Assert.Contains("Resolve(System.IServiceProvider) must return a named type", exception.Message);
    }
}
