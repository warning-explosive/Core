namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Transaction;
    using Basics;
    using Microsoft.Extensions.Logging;
    using Settings;
    using Sql.Extensions;
    using Sql.Model;

    internal static class RepositoryExtensions
    {
        private const string TransactionIdCommandText = "select txid_current()";

        internal static async Task<long> GetXid(
            this IAdvancedDatabaseTransaction transaction,
            OrmSettings settings,
            ILogger logger,
            CancellationToken token)
        {
            var dynamicValues = await transaction
               .Query(TransactionIdCommandText, settings, logger, token)
               .ConfigureAwait(false);

            var xid = (dynamicValues.SingleOrDefault() as IDictionary<string, object?>)?.SingleOrDefault().Value;

            return xid != null
                ? (long)xid
                : throw new InvalidOperationException("Unable to get txid_current()");
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
                           .Select(dependency => typeof(RepositoryExtensions)
                               .CallMethod(nameof(CreateMtmInstance))
                               .WithTypeArguments(
                                    column.MultipleRelationTable!,
                                    entity.GetType().ExtractGenericArgumentAt(typeof(IUniqueIdentified<>)),
                                    dependency.GetType().ExtractGenericArgumentAt(typeof(IUniqueIdentified<>)))
                               .WithArguments(entity.PrimaryKey, dependency.PrimaryKey)
                               .Invoke<IUniqueIdentified>()));
                };
            }
        }

        private static TMtm CreateMtmInstance<TMtm, TLeftKey, TRightKey>(
            TLeftKey leftKey,
            TRightKey rightKey)
            where TMtm : BaseMtmDatabaseEntity<TLeftKey, TRightKey>, new()
            where TLeftKey : notnull
            where TRightKey : notnull
        {
            return new TMtm
            {
                Left = leftKey,
                Right = rightKey
            };
        }
    }
}