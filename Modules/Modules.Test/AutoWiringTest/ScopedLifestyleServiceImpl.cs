namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class ScopedLifestyleServiceImpl : IScopedLifestyleService
    {
        public Task DoSmth()
        {
            return Task.Delay(50);
        }
    }
}