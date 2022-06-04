namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using System;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class ExternalResolvableOpenGeneric<T> : IProgress<T>,
                                                      IResolvable<IProgress<T>>
        where T : class
    {
        public void Report(T value)
        {
            throw new ArgumentException(nameof(ExternalResolvable), nameof(value));
        }
    }
}