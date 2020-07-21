namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringTest;

    [Lifestyle(EnLifestyle.Transient)]
    [Unregistered]
    internal class UnregisteredDecoratorWithExtraDependency : IServiceForInterceptionDecorator
    {
        private readonly IExtraDependency _extra;
        private readonly ImplementationExtra _implExtra;

        public UnregisteredDecoratorWithExtraDependency(IServiceForInterception decoratee,
                                                        IExtraDependency extra,
                                                        ImplementationExtra implExtra)
        {
            Decoratee = decoratee;
            _extra = extra;
            _implExtra = implExtra;
        }

        public IServiceForInterception Decoratee { get; }
    }
}