namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

using System.Threading.Tasks;
using System;

[Component(EnLifestyle.Scoped)]
internal class ScopedService : IScopedService
{
    public Task DoSmth()
    {
        return Task.Delay(TimeSpan.FromMilliseconds(10));
    }
}