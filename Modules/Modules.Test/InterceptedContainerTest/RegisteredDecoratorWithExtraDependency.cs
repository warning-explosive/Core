namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringTest;

    [Lifestyle(EnLifestyle.Transient)]
    internal class RegisteredDecoratorWithExtraDependency : IServiceForInterceptionDecorator,
                                                            IWithExtra
    {
        public RegisteredDecoratorWithExtraDependency(IServiceForInterception decoratee,
                                                      IExtraDependency extra,
                                                      ImplementationExtra implExtra)
        {
            Decoratee = decoratee;
            Extra = extra;
            ImplExtra = implExtra;
        }

        public IServiceForInterception Decoratee { get; }

        public IExtraDependency Extra { get; }

        public ImplementationExtra ImplExtra { get; }
    }
}