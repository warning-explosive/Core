namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringTest;

    [Lifestyle(EnLifestyle.Transient)]
    [Unregistered]
    internal class UnregisteredDecoratorWithExtraDependency : IServiceForInterceptionDecorator,
                                                              IWithExtra
    {
        public UnregisteredDecoratorWithExtraDependency(IServiceForInterception decoratee,
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