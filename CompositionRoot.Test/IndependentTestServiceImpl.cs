namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class IndependentTestServiceImpl : IIndependentTestService
    {
    }
}