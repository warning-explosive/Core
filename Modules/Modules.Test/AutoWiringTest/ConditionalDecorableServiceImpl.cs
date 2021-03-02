namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [TestCondition1]
    [Lifestyle(EnLifestyle.Transient)]
    internal class ConditionalDecorableServiceImpl : IConditionalDecorableService
    {
    }
}