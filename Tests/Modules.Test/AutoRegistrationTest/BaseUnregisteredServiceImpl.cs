namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Attributes;

    [SuppressMessage("Analysis", "CR1", Justification = "Unregistered for test reasons")]
    [UnregisteredComponent]
    internal class BaseUnregisteredServiceImpl : IUnregisteredService
    {
    }
}