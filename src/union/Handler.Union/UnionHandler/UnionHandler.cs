using System;

namespace GarageGroup.Infra;

public sealed partial class UnionHandler<T> : IHandler<T, Unit>
{
    public static UnionHandler<T> From(params IHandler<T, Unit>[] innerHandlers)
        =>
        new(
            innerHandlers ?? []);

    private readonly IHandler<T, Unit>[] innerHandlers;

    internal UnionHandler(params IHandler<T, Unit>[] innerHandlers)
        =>
        this.innerHandlers = innerHandlers;
}