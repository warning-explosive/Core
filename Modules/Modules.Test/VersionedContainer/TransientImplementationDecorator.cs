namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class TransientImplementationDecorator : TransientImplementation,
                                                      IDecorator<TransientImplementation>
    {
        public TransientImplementationDecorator(TransientImplementation decoratee)
        {
            Decoratee = decoratee;
        }

        public TransientImplementation Decoratee { get; }
    }
}