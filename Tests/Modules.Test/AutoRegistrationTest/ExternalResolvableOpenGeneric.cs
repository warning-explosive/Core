namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class ExternalResolvableOpenGeneric<T> : IProgress<T>,
                                                      IExternalResolvable<IProgress<T>>
        where T : class
    {
        public void Report(T value)
        {
            throw new ArgumentException(nameof(ExternalResolvable), nameof(value));
        }
    }
}