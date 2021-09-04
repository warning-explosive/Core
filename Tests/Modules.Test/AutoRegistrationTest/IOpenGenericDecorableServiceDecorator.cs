namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface IOpenGenericDecorableServiceDecorator<T> : IDecorator<IOpenGenericDecorableService<T>>
    {
    }
}