namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

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