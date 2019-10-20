namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class IndependentTestServiceImpl : IIndependentTestService
    {
        public string Do() => nameof(IndependentTestServiceImpl);
    }
}