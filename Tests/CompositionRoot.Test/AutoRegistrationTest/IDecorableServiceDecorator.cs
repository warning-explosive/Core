namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;

    internal interface IDecorableServiceDecorator : IDecorator<IDecorableService>
    {
    }
}