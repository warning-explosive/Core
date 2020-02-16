namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class ConcreteImplementationGenericService<T> : IResolvableImplementation
    {
        public ConcreteImplementationGenericService()
        {
        }
    }
}