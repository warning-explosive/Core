namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Unregistered]
    internal class UnregisteredImplementationExtraDecorator : ImplementationExtraDecorator
    {
        public UnregisteredImplementationExtraDecorator(ImplementationExtra decoratee)
            : base(decoratee)
        {
        }
    }
}