namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Unregistered]
    internal class AnotherUnregisteredImplementationExtraDecorator : ImplementationExtraDecorator
    {
        public AnotherUnregisteredImplementationExtraDecorator(ImplementationExtra decoratee)
            : base(decoratee)
        {
        }
    }
}