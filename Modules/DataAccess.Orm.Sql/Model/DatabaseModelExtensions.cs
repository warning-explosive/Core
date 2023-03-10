namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using Basics;

    internal static class DatabaseModelExtensions
    {
        private static readonly Type MultipleRelation = typeof(List<>);

        private static readonly Type[] SupportedMultipleRelations =
            new[]
            {
                typeof(IReadOnlyCollection<>)
            };

        private static readonly ConcurrentDictionary<Type, ColumnProperty[]> ColumnsCache
            = new ConcurrentDictionary<Type, ColumnProperty[]>();

        public static bool IsSupportedColumn(this PropertyInfo property)
        {
            var isItemTypeSupported = property.PropertyType.IsMultipleRelation(out var itemType)
                ? IsItemTypeSupported(itemType)
                : IsItemTypeSupported(property.PropertyType);

            return isItemTypeSupported || property.IsJsonColumn();

            static bool IsItemTypeSupported(Type type)
            {
                type = type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

                return type.IsEnum
                       || type == typeof(short)
                       || type == typeof(int)
                       || type == typeof(long)
                       || type == typeof(float)
                       || type == typeof(double)
                       || type == typeof(decimal)
                       || type == typeof(Guid)
                       || type == typeof(bool)
                       || type == typeof(string)
                       || type == typeof(byte[])
                       || type == typeof(DateTime)
                       || type == typeof(TimeSpan)
                       || type == TypeExtensions.FindType("System.Private.CoreLib System.DateOnly")
                       || type == TypeExtensions.FindType("System.Private.CoreLib System.TimeOnly")
                       || type.IsDatabaseEntity()
                       || type.IsInlinedObject();
            }
        }

        public static bool IsDatabaseEntity(this Type type)
        {
            return typeof(IDatabaseEntity).IsAssignableFrom(type);
        }

        public static bool IsInlinedObject(this Type type)
        {
            return typeof(IInlinedObject).IsAssignableFrom(type);
        }

        public static bool IsSqlView(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(ISqlView<>))
                   && type.IsConcreteType();
        }

        public static bool IsJsonColumn(this PropertyInfo property)
        {
            return property.GetCustomAttributes<JsonColumnAttribute>().Any()
                   && property.PropertyType != typeof(string);
        }

        public static bool IsMultipleRelation(this Type type, [NotNullWhen(true)] out Type? itemType)
        {
            itemType = SupportedMultipleRelations
                .Select(collection =>
                {
                    var collectionItemType = type.ExtractGenericArgumentAtOrSelf(collection);
                    var isCollection = collectionItemType != type;
                    return (collectionItemType, isCollection);
                })
                .Where(info => info.isCollection)
                .Select(info => info.collectionItemType)
                .FirstOrDefault();

            return itemType != null;
        }

        public static Type GetMultipleRelationItemType(this Type type)
        {
            return IsMultipleRelation(type, out var itemType)
                ? itemType
                : throw new InvalidOperationException($"Type {type} doesn't represent multiple relation");
        }

        public static Type ConstructMultipleRelationType(this Type itemType)
        {
            return itemType.IsConstructedOrNonGenericType()
                ? MultipleRelation.MakeGenericType(itemType)
                : throw new InvalidOperationException($"Type {itemType} should be non-generic or fully closed generic type");
        }

        public static bool IsMtmTable(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>));
        }

        public static IEnumerable<IUniqueIdentified> Flatten(
            this IModelProvider modelProvider,
            IUniqueIdentified source)
        {
            var visited = new HashSet<IUniqueIdentified>(new UniqueIdentifiedEqualityComparer());

            return source
                .Flatten(entity => visited.Add(entity)
                    ? UnfoldRelations(entity, modelProvider)
                    : Enumerable.Empty<IUniqueIdentified>());

            static IEnumerable<IUniqueIdentified> UnfoldRelations(
                IUniqueIdentified owner,
                IModelProvider modelProvider)
            {
                var type = owner.GetType();
                var table = modelProvider.Tables[type];

                if (table.IsMtmTable)
                {
                    return Enumerable.Empty<IUniqueIdentified>();
                }

                var relations = table
                    .Columns
                    .Values
                    .Where(column => column.IsRelation)
                    .Select(column => column.GetRelationValue(owner))
                    .Where(dependency => dependency != null)
                    .Select(dependency => dependency!);

                var multipleRelations = table
                    .Columns
                    .Values
                    .Where(column => column.IsMultipleRelation)
                    .SelectMany(column => column.GetMultipleRelationValue(owner));

                var mtms = table
                    .Columns
                    .Values
                    .Where(column => column.IsMultipleRelation)
                    .SelectMany(column => column
                        .GetMultipleRelationValue(owner)
                        .Select(dependency => column.CreateMtm(owner, dependency)));

                return relations.Concat(multipleRelations).Concat(mtms);
            }
        }

        public static ColumnProperty[] Columns(this Type type)
        {
            return ColumnsCache.GetOrAdd(type, key => GetColumns(key).ToArray());

            static IEnumerable<ColumnProperty> GetColumns(Type type)
            {
                return DeclaredColumns(type)
                    .Join(ReflectedColumns(type),
                        declared => declared.Name,
                        reflected => reflected.Name,
                        (declared, reflected) => new ColumnProperty(declared, reflected),
                        StringComparer.OrdinalIgnoreCase);
            }

            static IEnumerable<PropertyInfo> ReflectedColumns(Type type)
            {
                return type
                   .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty)
                   .Where(property => property.GetIsAccessible() && property.SetIsAccessible());
            }

            static IEnumerable<PropertyInfo> DeclaredColumns(Type type)
            {
                return type
                   .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.DeclaredOnly)
                   .Where(property => property.GetIsAccessible() && property.SetIsAccessible())
                   .Concat(BaseDeclaredColumns(type))
                   .Where(property => property.CanRead && (property.CanWrite || type.IsAnonymous()));
            }

            static IEnumerable<PropertyInfo> BaseDeclaredColumns(Type type)
            {
                return type.BaseType == null
                    ? Enumerable.Empty<PropertyInfo>()
                    : DeclaredColumns(type.BaseType);
            }
        }

        public static ColumnProperty Column(this Type type, string name)
        {
            return Columns(type).SingleOrDefault(property => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                   ?? throw new InvalidOperationException($"Unable to find column {name} in table {type.FullName}");
        }
    }
}