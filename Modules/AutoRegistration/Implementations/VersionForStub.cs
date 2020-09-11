namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;

    [ManualRegistration]
    internal class VersionForStub<TService> : IVersionFor<TService>
        where TService : class
    {
        public TService Version => default!;
    }
}