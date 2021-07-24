namespace SpaceEngineers.Core.GenericDomain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using Basics;

    /// <summary>
    /// DomainModelExtensions
    /// </summary>
    public static class DomainModelExtensions
    {
        private static readonly Type[] SupportedCollections =
            new[]
            {
                typeof(ICollection<>),
                typeof(IReadOnlyCollection<>)
            };

        /// <summary>
        /// Is model type supported
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Model type supported or not</returns>
        public static bool IsTypeSupported(this Type type)
        {
            return type.IsMultipleRelation(out var itemType)
                ? IsItemTypeSupported(itemType)
                : IsItemTypeSupported(type);

            static bool IsItemTypeSupported(Type type)
            {
                return type.IsPrimitive()
                       || type.IsEnumerationObject()
                       || type.IsEntity()
                       || type.IsValueObject();
            }
        }

        /// <summary>
        /// Is the type an entity
        /// </summary>
        /// <param name="type">type</param>
        /// <returns>Type is entity or not</returns>
        public static bool IsEntity(this Type type)
        {
            return typeof(IEntity).IsAssignableFrom(type);
        }

        /// <summary>
        /// Is the type an value object
        /// </summary>
        /// <param name="type">type</param>
        /// <returns>Type is value object or not</returns>
        public static bool IsValueObject(this Type type)
        {
            return typeof(IValueObject).IsAssignableFrom(type);
        }

        /// <summary>
        /// Is the type an enumeration object
        /// </summary>
        /// <param name="type">type</param>
        /// <returns>Type is enumeration object or not</returns>
        public static bool IsEnumerationObject(this Type type)
        {
            return typeof(EnumerationObject).IsAssignableFrom(type);
        }

        /// <summary>
        /// Is multiple relation (MtM, MtO)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="itemType">Item type</param>
        /// <returns>Type represents multiple relation or not</returns>
        [SuppressMessage("Analysis", "CA1021", Justification = "Desirable method name")]
        public static bool IsMultipleRelation(this Type type, [NotNullWhen(true)] out Type? itemType)
        {
            itemType = SupportedCollections
                .Select(collection =>
                {
                    var unwrapped = type.UnwrapTypeParameter(collection);
                    var success = unwrapped != type;
                    return (unwrapped, success);
                })
                .Where(pair => pair.success)
                .Select(pair => pair.unwrapped)
                .FirstOrDefault();

            return itemType != null;
        }
    }
}