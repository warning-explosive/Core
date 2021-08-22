namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [TestCondition1]
    [Component(EnLifestyle.Transient)]
    internal class ConditionalDecorableServiceImpl : IConditionalDecorableService
    {
    }
}