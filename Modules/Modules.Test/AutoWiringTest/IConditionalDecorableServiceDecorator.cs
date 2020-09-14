namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiringApi.Abstractions;

    internal interface IConditionalDecorableServiceDecorator<TAttribute> : IConditionalDecorator<IConditionalDecorableService, TAttribute>
        where TAttribute : Attribute
    {
    }
}