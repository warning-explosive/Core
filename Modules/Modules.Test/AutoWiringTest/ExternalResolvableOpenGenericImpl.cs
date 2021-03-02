namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [OpenGenericFallBack]
    internal class ExternalResolvableOpenGenericImpl<T> : IComparable<T>,
                                                          IExternalResolvable<IComparable<T>>
        where T : class
    {
        public int CompareTo(T? other)
        {
            throw new ArgumentException(nameof(ExternalResolvableImpl), nameof(other));
        }
    }
}