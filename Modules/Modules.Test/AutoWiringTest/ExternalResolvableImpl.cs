namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

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