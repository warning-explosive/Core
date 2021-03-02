namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

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