namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

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