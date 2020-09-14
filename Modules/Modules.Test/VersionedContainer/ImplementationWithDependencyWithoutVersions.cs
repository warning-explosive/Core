namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ImplementationWithDependencyWithoutVersions : IResolvable
    {
        public ImplementationWithDependencyWithoutVersions(IVersioned<IWithoutVersions> versioned)
        {
            Versioned = versioned;
        }

        public IVersioned<IWithoutVersions> Versioned { get; }
    }
}