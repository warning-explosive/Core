namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;

    internal interface IDecorableServiceDecorator : IDecorator<IDecorableService>
    {
    }
}