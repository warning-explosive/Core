namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System.Diagnostics.CodeAnalysis;
    using AutoWiring.Api.Attributes;

    [Unregistered]
    [SuppressMessage("Analysis", "CR1", Justification = "Unregistered for test reasons")]
    internal class BaseUnregisteredServiceImpl : IUnregisteredService
    {
    }
}