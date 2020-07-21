namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Unregistered]
    internal class UnregisteredExtraDependencyDecorator : IExtraDependencyDecorator
    {
        public UnregisteredExtraDependencyDecorator(IExtraDependency decoratee)
        {
            Decoratee = decoratee;
        }

        public IExtraDependency Decoratee { get; }
    }
}