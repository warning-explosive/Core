namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using AutoRegistration.Api.Attributes;
    using CrossCuttingConcerns.Api.Abstractions;

    [UnregisteredComponent]
    internal class StringFormatterMock : IStringFormatter
    {
        public string Format(object? value)
        {
            return value?.ToString() ?? "null";
        }
    }
}