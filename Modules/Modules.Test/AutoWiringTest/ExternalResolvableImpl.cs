namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ExternalResolvableImpl : IComparable<ExternalResolvableImpl>,
                                            IExternalResolvable<IComparable<ExternalResolvableImpl>>
    {
        public int CompareTo(ExternalResolvableImpl? other)
        {
            throw new ArgumentException(nameof(ExternalResolvableImpl), nameof(other));
        }
    }
}