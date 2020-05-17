namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [OpenGenericFallBack(typeof(IComparable<>))]
    internal class ExternalResolvableOpenGenericImpl<T> : IComparable<T>,
                                                          IExternalResolvable<IComparable<T>>
    {
        public int CompareTo(T other)
        {
            throw new ArgumentException(nameof(ExternalResolvableImpl), nameof(other));
        }
    }
}