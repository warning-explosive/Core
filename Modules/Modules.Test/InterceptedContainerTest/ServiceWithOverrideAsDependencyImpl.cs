namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ServiceWithOverrideAsDependencyImpl : IServiceWithOverrideAsDependency
    {
        public ServiceWithOverrideAsDependencyImpl(IServiceForInterception interception)
        {
            ServiceForInterception = interception;
        }

        public IServiceForInterception ServiceForInterception { get; }
    }
}