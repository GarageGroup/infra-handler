using System;

namespace GGroupp.Infra;

public sealed partial class UnionHandler<T> : IHandler<T>
{
    public static UnionHandler<T> From(params IHandler<T>[] innerHandlers)
        =>
        new(
            innerHandlers ?? Array.Empty<IHandler<T>>());

    private readonly IHandler<T>[] innerHandlers;

    internal UnionHandler(params IHandler<T>[] innerHandlers)
        =>
        this.innerHandlers = innerHandlers;
}