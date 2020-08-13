namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiringApi.Abstractions;

    internal interface ICollectionResolvableConditionDecorableServiceDecorator<TAttribute> : IConditionalCollectionDecorator<ICollectionResolvableConditionDecorableService, TAttribute>
        where TAttribute : Attribute
    {
    }
}