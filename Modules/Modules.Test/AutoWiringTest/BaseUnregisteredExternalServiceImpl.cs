namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System.Diagnostics.CodeAnalysis;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [SuppressMessage("Analysis", "CR1", Justification = "Unregistered for test reasons")]
    [Component(EnLifestyle.Transient, EnComponentRegistrationKind.Unregistered)]
    internal class BaseUnregisteredExternalServiceImpl : IUnregisteredExternalService
    {
    }
}