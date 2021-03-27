namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ConcreteImplementationGenericService<T> : IResolvable
    {
        public ConcreteImplementationGenericService()
        {
        }
    }
}