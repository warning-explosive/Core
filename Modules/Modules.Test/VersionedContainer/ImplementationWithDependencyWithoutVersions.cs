namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

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