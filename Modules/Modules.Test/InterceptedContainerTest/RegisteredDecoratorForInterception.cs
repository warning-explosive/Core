namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class RegisteredDecoratorForInterception : IServiceForInterceptionDecorator
    {
        public RegisteredDecoratorForInterception(IServiceForInterception decoratee)
        {
            Decoratee = decoratee;
        }

        public IServiceForInterception Decoratee { get; }
    }
}