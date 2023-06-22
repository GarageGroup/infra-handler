using System;
using PrimeFuncPack;

namespace GarageGroup.Infra;

partial class UnionHandlerDependency
{
    public static Dependency<UnionHandler<T>> Join<THandler1, THandler2, T>(
        this Dependency<THandler1, THandler2> dependency)
        where THandler1 : IHandler<T, Unit>
        where THandler2 : IHandler<T, Unit>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Fold(CreateUnionHandler);

        static UnionHandler<T> CreateUnionHandler(THandler1 handler1, THandler2 handler2)
        {
            ArgumentNullException.ThrowIfNull(handler1);
            ArgumentNullException.ThrowIfNull(handler2);

            return new(handler1, handler2);
        }
    }
}