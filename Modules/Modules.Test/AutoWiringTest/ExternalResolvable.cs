namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ExternalResolvable : IComparable<ExternalResolvable>,
                                        IExternalResolvable<IComparable<ExternalResolvable>>
    {
        public int CompareTo(ExternalResolvable? other)
        {
            throw new ArgumentException(nameof(ExternalResolvable), nameof(other));
        }
    }
}