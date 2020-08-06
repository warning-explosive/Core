namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    [ManualRegistration]
    internal class VersionForStub<TService> : IVersionFor<TService>
        where TService : class
    {
        public TService Version => default!;
    }
}