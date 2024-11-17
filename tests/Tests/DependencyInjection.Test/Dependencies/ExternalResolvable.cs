namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

using System;

[Component(EnLifestyle.Transient)]
internal class ExternalResolvable : IProgress<ExternalResolvable>
{
    public void Report(ExternalResolvable value)
    {
        throw new ArgumentException(nameof(ExternalResolvable), nameof(value));
    }
}