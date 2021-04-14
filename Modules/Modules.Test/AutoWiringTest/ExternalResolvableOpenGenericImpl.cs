namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient, EnComponentKind.OpenGenericFallback)]
    internal class ExternalResolvableOpenGenericImpl<T> : IProgress<T>,
                                                          IExternalResolvable<IProgress<T>>
        where T : class
    {
        public void Report(T value)
        {
            throw new ArgumentException(nameof(ExternalResolvableImpl), nameof(value));
        }
    }
}