namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class TransientVersionedServiceDecorator : ITransientVersionedService,
                                                        IDecorator<ITransientVersionedService>
    {
        public TransientVersionedServiceDecorator(ITransientVersionedService decoratee)
        {
            Decoratee = decoratee;
        }

        public ITransientVersionedService Decoratee { get; }
    }
}