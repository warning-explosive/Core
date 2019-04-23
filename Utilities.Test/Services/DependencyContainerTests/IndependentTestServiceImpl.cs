namespace SpaceEngineers.Core.Utilities.Test.Services.DependencyContainerTests
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class IndependentTestServiceImpl : IIndependentTestService
    {
        public string Do() => nameof(IndependentTestServiceImpl);
    }
}