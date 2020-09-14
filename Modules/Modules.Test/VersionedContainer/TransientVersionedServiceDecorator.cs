namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

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