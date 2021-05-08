namespace SpaceEngineers.Core.CrossCuttingConcerns.Internals
{
    using System;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class StringFormatter : IStringFormatter
    {
        private readonly IDependencyContainer _dependencyContainer;

        public StringFormatter(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Format(object? value)
        {
            var type = (value?.GetType() ?? typeof(object)).UnwrapTypeParameter(typeof(Nullable<>));

            return GetType()
                .CallMethod(nameof(Format))
                .WithTypeArgument(type)
                .WithArgument(value)
                .ForInstance(this)
                .Invoke<string>();
        }

        private string Format<T>(T value)
        {
            IStringFormatter<T> formatter = _dependencyContainer.Resolve<IStringFormatter<T>>();
            return formatter.Format(value);
        }
    }
}