namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonVersionedServiceDecorator : ISingletonVersionedService,
                                                        IDecorator<ISingletonVersionedService>
    {
        public SingletonVersionedServiceDecorator(ISingletonVersionedService decoratee)
        {
            Decoratee = decoratee;
        }

        public ISingletonVersionedService Decoratee { get; }
    }
}