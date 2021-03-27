namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ConcreteImplementationService : IResolvable
    {
        public ConcreteImplementationService()
        {
        }
    }
}