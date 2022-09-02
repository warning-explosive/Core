namespace SpaceEngineers.Core.CrossCuttingConcerns.StringFormatter
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ObjectStringFormatter<T> : IStringFormatter<T>,
                                              IResolvable<IStringFormatter<T>>
    {
        private const string Null = "null";

        public string Format(T? value)
        {
            return value?.ToString() ?? Null;
        }
    }
}