namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Unregistered]
    internal class UnregisteredDecoratorCyclicReferenceProxy : IServiceForInterceptionDecorator
    {
        private readonly IServiceWithDecoratedDependency _cyclicReference;

        public UnregisteredDecoratorCyclicReferenceProxy(IServiceForInterception decoratee, IServiceWithDecoratedDependency cyclicReference)
        {
            Decoratee = decoratee;
            _cyclicReference = cyclicReference;
        }

        public IServiceForInterception Decoratee { get; }
    }
}