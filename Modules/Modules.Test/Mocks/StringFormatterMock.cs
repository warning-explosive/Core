namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;

    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.Override)]
    internal class StringFormatterMock : IStringFormatter
    {
        public string Format(object? value)
        {
            return value?.ToString() ?? "null";
        }
    }
}