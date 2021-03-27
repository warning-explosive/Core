namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class ExternalResolvableImpl : IComparable<ExternalResolvableImpl>,
                                            IExternalResolvable<IComparable<ExternalResolvableImpl>>
    {
        public int CompareTo(ExternalResolvableImpl? other)
        {
            throw new ArgumentException(nameof(ExternalResolvableImpl), nameof(other));
        }
    }
}