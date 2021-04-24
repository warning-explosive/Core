namespace SpaceEngineers.Core.CrossCuttingConcerns
{
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.ManuallyRegistered)]
    internal class ObjectStringFormatter<T> : IStringFormatter<T>
    {
        private const string Null = "null";

        public string Format(T? value)
        {
            return value?.ToString() ?? Null;
        }
    }
}