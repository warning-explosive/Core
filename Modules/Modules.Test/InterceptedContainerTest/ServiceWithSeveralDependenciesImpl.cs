namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ServiceWithSeveralDependenciesImpl : IServiceWithSeveralDependencies
    {
        public ServiceWithSeveralDependenciesImpl(IServiceForInterception serviceForInterception,
                                                  IServiceWithDecoratedDependency serviceWithDecoratedDependency)
        {
            ServiceForInterception = serviceForInterception;
            ServiceWithDecoratedDependency = serviceWithDecoratedDependency;
        }

        public IServiceForInterception ServiceForInterception { get; }

        public IServiceWithDecoratedDependency ServiceWithDecoratedDependency { get; }
    }
}