namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class VersionedAndDecoratedImplementationDecorator : VersionedAndDecoratedImplementation,
                                                                  IDecorator<VersionedAndDecoratedImplementation>
    {
        public VersionedAndDecoratedImplementationDecorator(VersionedAndDecoratedImplementation decoratee)
        {
            Decoratee = decoratee;
        }

        public VersionedAndDecoratedImplementation Decoratee { get; }
    }
}