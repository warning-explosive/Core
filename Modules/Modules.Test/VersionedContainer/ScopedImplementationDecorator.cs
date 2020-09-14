namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class ScopedImplementationDecorator : ScopedImplementation,
                                                   IDecorator<ScopedImplementation>
    {
        public ScopedImplementationDecorator(ScopedImplementation decoratee)
        {
            Decoratee = decoratee;
        }

        public ScopedImplementation Decoratee { get; }
    }
}