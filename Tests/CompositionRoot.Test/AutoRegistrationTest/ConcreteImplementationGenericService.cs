namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ConcreteImplementationGenericService<T> : IResolvable<ConcreteImplementationGenericService<T>>
    {
        public ConcreteImplementationGenericService()
        {
        }
    }
}