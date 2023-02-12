namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Connection;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using Transaction;

    internal static class RepositoryExtensions
    {
        private const string TransactionIdCommandText = "select txid_current()";

        internal static Task<long> GetXid(
            this IDatabaseConnectionProvider connectionProvider,
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            return connectionProvider.ExecuteScalar<long>(
                transaction,
                new SqlCommand(TransactionIdCommandText, Array.Empty<SqlCommandParameter>()),
                token);
        }

        internal static IEnumerable<IUniqueIdentified> Flatten(
            this IUniqueIdentified entity,
            IModelProvider modelProvider)
        {
            return UnfoldCycleRelations(entity, modelProvider, new List<IUniqueIdentified>())
               .SelectMany(e => e.Flatten(UnfoldMtmRelations(modelProvider)));

            static IEnumerable<IUniqueIdentified> UnfoldCycleRelations(
                IUniqueIdentified entity,
                IModelProvider modelProvider,
                ICollection<IUniqueIdentified> visited)
            {
                visited.Add(entity);

                return entity
                   .Flatten(source => UnfoldRelations(modelProvider)(source)
                       .Where(dependency => !visited.Contains(dependency)));
            }

            static Func<IUniqueIdentified, IEnumerable<IUniqueIdentified>> UnfoldRelations(
                IModelProvider modelProvider)
            {
                return entity =>
                {
                    var type = entity.GetType();
                    var table = modelProvider.Tables[type];

                    return table
                       .Columns
                       .Values
                       .Where(column => column.IsRelation)
                       .Select(column => column.GetRelationValue(entity))
                       .Where(dependency => dependency != null)
                       .Select(dependency => dependency!)
                       .Concat(table
                           .Columns
                           .Values
                           .Where(column => column.IsMultipleRelation)
                           .SelectMany(column => column.GetMultipleRelationValue(entity)));
                };
            }

            static Func<IUniqueIdentified, IEnumerable<IUniqueIdentified>> UnfoldMtmRelations(
                IModelProvider modelProvider)
            {
                return entity =>
                {
                    var type = entity.GetType();
                    var table = modelProvider.Tables[type];

                    return table
                       .Columns
                       .Values
                       .Where(column => column.IsMultipleRelation)
                       .SelectMany(column => column
                           .GetMultipleRelationValue(entity)
                           .Select(dependency => column.CreateMtm(entity, dependency)));
                };
            }
        }
    }
}