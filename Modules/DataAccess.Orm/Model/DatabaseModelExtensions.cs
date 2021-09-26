namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Api.Model;
    using Basics;

    /// <summary>
    /// DatabaseModelExtensions
    /// </summary>
    public static class DatabaseModelExtensions
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
                    || type.IsDatabaseEntity()
                    || type.IsInlinedObject();
            }
        }

        /// <summary>
        /// Is type a database entity
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type is an database entity or not</returns>
        public static bool IsDatabaseEntity(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>));
        }

        /// <summary>
        /// Is type an inlined object
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type is an inlined object or not</returns>
        public static bool IsInlinedObject(this Type type)
        {
            return typeof(IInlinedObject).IsAssignableFrom(type);
        }

        /// <summary>
        /// Is multiple relation (MtM, MtO)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="itemType">Item type</param>
        /// <returns>Type represents multiple relation or not</returns>
        [SuppressMessage("Analysis", "CA1021", Justification = "desired method name")]
        public static bool IsMultipleRelation(this Type type, [NotNullWhen(true)] out Type? itemType)
        {
            itemType = SupportedCollections
                .Select(collection =>
                {
                    var collectionItemType = type.UnwrapTypeParameter(collection);
                    var isCollection = collectionItemType != type;
                    return (collectionItemType, isCollection);
                })
                .Where(info => info.isCollection)
                .Select(info => info.collectionItemType)
                .FirstOrDefault();

            return itemType != null;
        }
    }
}