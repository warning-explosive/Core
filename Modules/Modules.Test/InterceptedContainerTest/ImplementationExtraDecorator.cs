namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    internal abstract class ImplementationExtraDecorator : ImplementationExtra,
                                                           IDecorator<ImplementationExtra>
    {
        protected ImplementationExtraDecorator(ImplementationExtra decoratee)
        {
            Decoratee = decoratee;
        }

        public ImplementationExtra Decoratee { get; }
    }
}