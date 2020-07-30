namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

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