namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

using System;

[Component(EnLifestyle.Transient)]
internal class ExternalResolvableOpenGeneric<T> : IProgress<T>
    where T : class
{
    public void Report(T value)
    {
        throw new ArgumentException(nameof(ExternalResolvable), nameof(value));
    }
}