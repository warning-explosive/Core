namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [TestConditionAttribute1]
    [Lifestyle(EnLifestyle.Transient)]
    internal class ConditionalDecorableServiceImpl : IConditionalDecorableService
    {
    }
}