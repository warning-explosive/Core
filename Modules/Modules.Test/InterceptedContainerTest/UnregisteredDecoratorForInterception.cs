namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Unregistered]
    internal class UnregisteredDecoratorForInterception : IServiceForInterceptionDecorator
    {
        public UnregisteredDecoratorForInterception(IServiceForInterception decoratee)
        {
            Decoratee = decoratee;
        }

        public IServiceForInterception Decoratee { get; }
    }
}