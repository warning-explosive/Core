namespace SpaceEngineers.Core.AutoWiringApi.Attributes
{
    using System;

    /// <summary>
    /// Open generic fallback attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OpenGenericFallBackAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="serviceType">serviceType</param>
        public OpenGenericFallBackAttribute(Type serviceType)
        {
            var serviceTypeIsValid = serviceType.IsInterface
                                  && serviceType.IsGenericType
                                  && serviceType.IsGenericTypeDefinition;

            if (!serviceTypeIsValid)
            {
                throw new ArgumentException($"Argument of {nameof(OpenGenericFallBackAttribute)} must be an open-generic interface",
                                            nameof(serviceType));
            }

            ServiceType = serviceType;
        }

        /// <summary>
        /// Service type
        /// </summary>
        public Type ServiceType { get; }
    }
}