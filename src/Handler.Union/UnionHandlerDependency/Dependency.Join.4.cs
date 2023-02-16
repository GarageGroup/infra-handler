using System;
using PrimeFuncPack;

namespace GGroupp.Infra;

partial class UnionHandlerDependency
{
    public static Dependency<UnionHandler<T>> Join<THandler1, THandler2, THandler3, THandler4, T>(
        this Dependency<THandler1, THandler2, THandler3, THandler4> dependency)
        where THandler1 : IHandler<T>
        where THandler2 : IHandler<T>
        where THandler3 : IHandler<T>
        where THandler4 : IHandler<T>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Fold(CreateUnionHandler);

        static UnionHandler<T> CreateUnionHandler(THandler1 handler1, THandler2 handler2, THandler3 handler3, THandler4 handler4)
        {
            ArgumentNullException.ThrowIfNull(handler1);
            ArgumentNullException.ThrowIfNull(handler2);
            ArgumentNullException.ThrowIfNull(handler3);
            ArgumentNullException.ThrowIfNull(handler4);

            return new(handler1, handler2, handler3, handler4);
        }
    }
}