namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ServiceForInterceptionImpl : IServiceForInterception,
                                                IWithExtra
    {
        public ServiceForInterceptionImpl(IExtraDependency extra, ImplementationExtra implExtra)
        {
            Extra = extra;
            ImplExtra = implExtra;
        }

        public IExtraDependency Extra { get; }

        public ImplementationExtra ImplExtra { get; }
    }
}