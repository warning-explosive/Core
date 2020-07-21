namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringTest;

    [Lifestyle(EnLifestyle.Transient)]
    internal class RegisteredDecoratorWithExtraDependency : IServiceForInterceptionDecorator
    {
        private readonly IExtraDependency _extra;
        private readonly ImplementationExtra _implExtra;

        public RegisteredDecoratorWithExtraDependency(IServiceForInterception decoratee,
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