namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonImplementationDecorator : SingletonImplementation,
                                                      IDecorator<SingletonImplementation>
    {
        public SingletonImplementationDecorator(SingletonImplementation decoratee)
        {
            Decoratee = decoratee;
        }

        public SingletonImplementation Decoratee { get; }
    }
}