namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class RegisteredImplementationExtraDecorator : ImplementationExtraDecorator
    {
        public RegisteredImplementationExtraDecorator(ImplementationExtra decoratee)
            : base(decoratee)
        {
        }
    }
}