namespace SpaceEngineers.Core.CrossCuttingConcerns
{
    using System;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    /// <inheritdoc />
    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.ManuallyRegistered)]
    public class StringFormatter : IStringFormatter
    {
        private readonly IDependencyContainer _dependencyContainer;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        public StringFormatter(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <inheritdoc />
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