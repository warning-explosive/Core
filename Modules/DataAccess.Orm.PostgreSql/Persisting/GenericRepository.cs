namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using CrossCuttingConcerns.Settings;
    using Settings;
    using Sql.Extensions;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Extensions;
    using Transaction;

    [Component(EnLifestyle.Scoped)]
    internal class GenericRepository<TEntity, TKey> : IRepository<TEntity, TKey>,
                                                      IResolvable<IRepository<TEntity, TKey>>
        where TEntity : IDatabaseEntity<TKey>
        where TKey : notnull
    {
        private const string UpdateValueQueryFormat = @"update ""{0}"".""{1}"" a set {2} where {3}";
        private const string SetExpressionFormat = @"{0} = {1}";

        private const string DeleteValueQueryFormat = @"delete ""{0}"".""{1}"" a where {2}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly IRepository _repository;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IExpressionTranslator _expressionTranslator;

        public GenericRepository(
            IDependencyContainer dependencyContainer,
            IRepository repository,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelProvider modelProvider,
            IAdvancedDatabaseTransaction transaction,
            IExpressionTranslator expressionTranslator)
        {
            _dependencyContainer = dependencyContainer;
            _repository = repository;
            _settingsProvider = settingsProvider;
            _modelProvider = modelProvider;
            _transaction = transaction;
            _expressionTranslator = expressionTranslator;
        }

        public Task<long> Insert(
            IReadOnlyCollection<TEntity> entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token)
        {
            return _repository.Insert(
                entities.Cast<IUniqueIdentified>().ToArray(),
                insertBehavior,
                token);
        }

        public async Task<long> Update<TValue>(
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            var visitor = new ExtractMemberChainExpressionVisitor();
            _ = visitor.Visit(accessor);

            var column = new ColumnInfo(
                table,
                visitor.Chain.Select(property => new ColumnProperty(property, property)).ToArray(),
                _modelProvider);

            if (column.IsMultipleRelation)
            {
                throw new NotSupportedException($"Unable to update multiple relation: {column.Name}");
            }

            var columnExpression = @$"""{column.Name}""";

            var valueIntermediateExpression = _expressionTranslator.Translate(valueProducer);

            var valueExpression = InlineQueryParameters(
                _dependencyContainer,
                valueIntermediateExpression.Translate(_dependencyContainer, 0),
                valueIntermediateExpression.ExtractQueryParameters());

            var predicateIntermediateExpression = _expressionTranslator.Translate(predicate);

            var predicateExpression = InlineQueryParameters(
                _dependencyContainer,
                predicateIntermediateExpression.Translate(_dependencyContainer, 0),
                predicateIntermediateExpression.ExtractQueryParameters());

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Name,
                SetExpressionFormat.Format(columnExpression, valueExpression),
                predicateExpression);

            try
            {
                var version = await _transaction
                   .GetXid(settings, token)
                   .ConfigureAwait(false);

                var affectedRowsCount = await _transaction
                    .InvokeScalar(commandText, settings, token)
                    .ConfigureAwait(false);

                var change = new UpdateEntityChange<TEntity, TKey, TValue>(
                    version,
                    affectedRowsCount,
                    accessor,
                    valueProducer,
                    predicate);

                _transaction.CollectChange(change);

                return affectedRowsCount;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }
        }

        public async Task<long> Delete(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
        {
            // TODO: #178 - add delete behaviors
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            var predicateIntermediateExpression = _expressionTranslator.Translate(predicate);

            var predicateExpression = InlineQueryParameters(
                _dependencyContainer,
                predicateIntermediateExpression.Translate(_dependencyContainer, 0),
                predicateIntermediateExpression.ExtractQueryParameters());

            var commandText = DeleteValueQueryFormat.Format(
                table.Type,
                table.Name,
                predicateExpression);

            try
            {
                var version = await _transaction
                   .GetXid(settings, token)
                   .ConfigureAwait(false);

                var affectedRowsCount = await _transaction
                    .InvokeScalar(commandText, settings, token)
                    .ConfigureAwait(false);

                var change = new DeleteEntityChange<TEntity, TKey>(
                    predicate,
                    version,
                    affectedRowsCount);

                _transaction.CollectChange(change);

                return affectedRowsCount;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }
        }

        private static string InlineQueryParameters(
            IDependencyContainer dependencyContainer,
            string query,
            IReadOnlyDictionary<string, object?> queryParameters)
        {
            foreach (var (name, value) in queryParameters)
            {
                if (!query.Contains($"@{name}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                query = query.Replace($"@{name}", value.QueryParameterSqlExpression(dependencyContainer), StringComparison.OrdinalIgnoreCase);
            }

            return query;
        }
    }
}