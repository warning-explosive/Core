namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface IOpenGenericDecorableServiceDecorator<T> : IDecorator<IOpenGenericDecorableService<T>>
    {
    }
}