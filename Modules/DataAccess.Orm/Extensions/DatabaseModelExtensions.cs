namespace SpaceEngineers.Core.DataAccess.Orm.Extensions
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
        private static readonly Type MultipleRelation = typeof(List<>);

        private static readonly Type[] SupportedMultipleRelations =
            new[]
            {
                typeof(IReadOnlyCollection<>)
            };

        /// <summary>
        /// Is model type supported
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Model type supported or not</returns>
        public static bool IsTypeSupported(this Type type)
        {
            type = type.UnwrapTypeParameter(typeof(Nullable<>));

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
        [SuppressMessage("Analysis", "CA1021", Justification = "desired method signature")]
        public static bool IsMultipleRelation(this Type type, [NotNullWhen(true)] out Type? itemType)
        {
            itemType = SupportedMultipleRelations
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

        /// <summary>
        /// Get item type of multiple relation
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Item type</returns>
        public static Type GetMultipleRelationItemType(this Type type)
        {
            return IsMultipleRelation(type, out var itemType)
                ? itemType
                : throw new InvalidOperationException($"Type {type} doesn't represent multiple relation");
        }

        /// <summary>
        /// Constructs multiple relation type
        /// </summary>
        /// <param name="itemType">Item type</param>
        /// <returns>Multiple relation type</returns>
        public static Type ConstructMultipleRelationType(this Type itemType)
        {
            return !itemType.IsGenericType || itemType.IsConstructedGenericType
                ? MultipleRelation.MakeGenericType(itemType)
                : throw new InvalidOperationException($"Type {itemType} should be non-generic or fully closed generic type");
        }
    }
}