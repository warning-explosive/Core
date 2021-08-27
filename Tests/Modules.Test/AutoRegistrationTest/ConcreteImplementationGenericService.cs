namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ConcreteImplementationGenericService<T> : IResolvable
    {
        public ConcreteImplementationGenericService()
        {
        }
    }
}