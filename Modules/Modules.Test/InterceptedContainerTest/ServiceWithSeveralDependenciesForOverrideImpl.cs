namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ServiceWithSeveralDependenciesForOverrideImpl : IServiceWithSeveralDependenciesForOverride
    {
        public ServiceWithSeveralDependenciesForOverrideImpl(IServiceForInterception serviceForInterception,
                                                             IServiceWithOverrideAsDependency serviceWithOverrideAsDependency)
        {
            ServiceForInterception = serviceForInterception;
            ServiceWithOverrideAsDependency = serviceWithOverrideAsDependency;
        }

        public IServiceForInterception ServiceForInterception { get; }

        public IServiceWithOverrideAsDependency ServiceWithOverrideAsDependency { get; }
    }
}