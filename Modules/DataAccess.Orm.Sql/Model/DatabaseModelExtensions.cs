namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using Api.Sql;
    using Basics;

    /// <summary>
    /// DatabaseModelExtensions
    /// </summary>
    public static class DatabaseModelExtensions
    {
        /// <summary>
        /// Does type represent sql view
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Check result</returns>
        public static bool IsSqlView(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(ISqlView<>))
                && type.IsConcreteType();
        }

        /// <summary>
        /// Does type represent mtm table
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Check result</returns>
        public static bool IsMtmTable(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>));
        }

        /// <summary>
        /// Flattens database entity's object tree
        /// </summary>
        /// <param name="modelProvider">IModelProvider</param>
        /// <param name="source">IUniqueIdentified</param>
        /// <returns>Flatten collection</returns>
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
    }
}