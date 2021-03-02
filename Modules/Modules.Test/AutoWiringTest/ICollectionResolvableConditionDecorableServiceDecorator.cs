namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiring.Api.Abstractions;

    internal interface ICollectionResolvableConditionDecorableServiceDecorator<TAttribute> : IConditionalCollectionDecorator<ICollectionResolvableConditionDecorableService, TAttribute>
        where TAttribute : Attribute
    {
    }
}