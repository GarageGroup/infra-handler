using System;
using PrimeFuncPack;

namespace GGroupp.Infra;

partial class UnionHandlerDependency
{
    public static Dependency<UnionHandler<T>> Join<THandler1, THandler2, THandler3, THandler4, THandler5, THandler6, THandler7, THandlerRest, T>(
        this Dependency<THandler1, THandler2, THandler3, THandler4, THandler5, THandler6, THandler7, THandlerRest> dependency)
        where THandler1 : IHandler<T>
        where THandler2 : IHandler<T>
        where THandler3 : IHandler<T>
        where THandler4 : IHandler<T>
        where THandler5 : IHandler<T>
        where THandler6 : IHandler<T>
        where THandler7 : IHandler<T>
        where THandlerRest : IHandler<T>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Fold(CreateUnionHandler);

        static UnionHandler<T> CreateUnionHandler(
            THandler1 handler1,
            THandler2 handler2,
            THandler3 handler3,
            THandler4 handler4,
            THandler5 handler5,
            THandler6 handler6,
            THandler7 handler7,
            THandlerRest handlerRest)
        {
            ArgumentNullException.ThrowIfNull(handler1);
            ArgumentNullException.ThrowIfNull(handler2);
            ArgumentNullException.ThrowIfNull(handler3);
            ArgumentNullException.ThrowIfNull(handler4);
            ArgumentNullException.ThrowIfNull(handler5);
            ArgumentNullException.ThrowIfNull(handler6);
            ArgumentNullException.ThrowIfNull(handler7);
            ArgumentNullException.ThrowIfNull(handlerRest);

            return new(handler1, handler2, handler3, handler4, handler5, handler6, handler7, handlerRest);
        }
    }
}