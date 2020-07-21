namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ServiceForInterceptionImpl : IServiceForInterception
    {
        private readonly IExtraDependency _extra;
        private readonly ImplementationExtra _implExtra;

        public ServiceForInterceptionImpl(IExtraDependency extra, ImplementationExtra implExtra)
        {
            _extra = extra;
            _implExtra = implExtra;
        }
    }
}