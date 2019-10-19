namespace SpaceEngineers.Core.CompositionRoot.Test.DependencyContainerTests
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class IndependentTestServiceImpl : IIndependentTestService
    {
        public string Do() => nameof(IndependentTestServiceImpl);
    }
}