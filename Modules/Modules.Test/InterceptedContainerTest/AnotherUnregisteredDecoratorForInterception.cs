namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Unregistered]
    internal class AnotherUnregisteredDecoratorForInterception : IServiceForInterceptionDecorator
    {
        public AnotherUnregisteredDecoratorForInterception(IServiceForInterception decoratee)
        {
            Decoratee = decoratee;
        }

        public IServiceForInterception Decoratee { get; }
    }
}