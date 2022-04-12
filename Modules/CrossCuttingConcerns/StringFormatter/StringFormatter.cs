namespace SpaceEngineers.Core.CrossCuttingConcerns.StringFormatter
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class StringFormatter : IStringFormatter,
                                     IResolvable<IStringFormatter>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public StringFormatter(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Format(object? value)
        {
            var type = (value?.GetType() ?? typeof(object)).UnwrapTypeParameter(typeof(Nullable<>));

            return _dependencyContainer
                .ResolveGeneric(typeof(IStringFormatter<>), type)
                .CallMethod(nameof(IStringFormatter<object>.Format))
                .WithArgument(value)
                .Invoke<string>();
        }
    }
}