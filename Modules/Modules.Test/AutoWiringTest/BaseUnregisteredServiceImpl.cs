namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System.Diagnostics.CodeAnalysis;
    using AutoWiringApi.Attributes;

    [Unregistered]
    [SuppressMessage("Analysis", "CR1", Justification = "Unregistered for test reasons")]
    internal class BaseUnregisteredServiceImpl : IUnregisteredService
    {
    }
}