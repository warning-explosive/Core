namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class ExternalResolvable : IProgress<ExternalResolvable>,
                                        IResolvable<IProgress<ExternalResolvable>>
    {
        public void Report(ExternalResolvable value)
        {
            throw new ArgumentException(nameof(ExternalResolvable), nameof(value));
        }
    }
}