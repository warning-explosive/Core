namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using System;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

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