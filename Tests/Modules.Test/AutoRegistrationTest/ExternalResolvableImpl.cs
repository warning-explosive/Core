namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class ExternalResolvableImpl : IProgress<ExternalResolvableImpl>,
                                            IExternalResolvable<IProgress<ExternalResolvableImpl>>
    {
        public void Report(ExternalResolvableImpl value)
        {
            throw new ArgumentException(nameof(ExternalResolvableImpl), nameof(value));
        }
    }
}