namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [TestCondition1]
    [Lifestyle(EnLifestyle.Transient)]
    internal class ConditionalDecorableServiceImpl : IConditionalDecorableService
    {
    }
}