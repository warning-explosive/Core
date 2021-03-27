namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [TestCondition1]
    [Component(EnLifestyle.Transient)]
    internal class ConditionalDecorableServiceImpl : IConditionalDecorableService
    {
    }
}