namespace SpaceEngineers.Core.CrossCuttingConcerns.Internals
{
    using System.Collections.Generic;
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class GenericObjectBuilder<T> : IObjectBuilder<T>
    {
        private readonly IObjectBuilder _objectBuilder;

        public GenericObjectBuilder(IObjectBuilder objectBuilder)
        {
            _objectBuilder = objectBuilder;
        }

        public T? Build(IDictionary<string, object>? values = null)
        {
            var built = _objectBuilder.Build(typeof(T), values);

            return built != null
                ? (T)built
                : default;
        }
    }
}