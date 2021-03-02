namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiring.Api.Abstractions;

    internal interface IConditionalDecorableServiceDecorator<TAttribute> : IConditionalDecorator<IConditionalDecorableService, TAttribute>
        where TAttribute : Attribute
    {
    }
}