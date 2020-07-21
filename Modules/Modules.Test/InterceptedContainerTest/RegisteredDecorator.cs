namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Dependency(typeof(RegisteredDecoratorWithExtraDependency))]
    internal class RegisteredDecorator : IServiceForInterceptionDecorator
    {
        public RegisteredDecorator(IServiceForInterception decoratee)
        {
            Decoratee = decoratee;
        }

        public IServiceForInterception Decoratee { get; }
    }
}