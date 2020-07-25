namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class RegisteredExtraDependencyDecorator : IExtraDependencyDecorator
    {
        public RegisteredExtraDependencyDecorator(IExtraDependency decoratee)
        {
            Decoratee = decoratee;
        }

        public IExtraDependency Decoratee { get; }
    }
}