namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Extensions;
    using Microsoft.Extensions.Logging;
    using Orm.Connection;
    using Settings;
    using Sql.Extensions;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Extensions;
    using Transaction;

    [SuppressMessage("Analysis", "CA1506", Justification = "Infrastructural code")]
    [Component(EnLifestyle.Singleton)]
    internal class GenericRepository<TEntity> : IRepository<TEntity>,
                                                IResolvable<IRepository<TEntity>>
        where TEntity : IDatabaseEntity
    {
        private const string UpdateValueQueryFormat = @"update ""{0}"".""{1}"" a set {2} where {3}";
        private const string SetExpressionFormat = @"{0} = {1}";

        private const string DeleteValueQueryFormat = @"delete from ""{0}"".""{1}"" a where {2}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly IRepository _repository;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;
        private readonly IExpressionTranslator _expressionTranslator;
        private readonly IDatabaseImplementation _databaseImplementation;
        private readonly ILogger _logger;

        public GenericRepository(
            IDependencyContainer dependencyContainer,
            IRepository repository,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelProvider modelProvider,
            ISqlExpressionTranslatorComposite sqlExpressionTranslator,
            IExpressionTranslator expressionTranslator,
            IDatabaseImplementation databaseImplementation,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _repository = repository;
            _settingsProvider = settingsProvider;
            _modelProvider = modelProvider;
            _sqlExpressionTranslator = sqlExpressionTranslator;
            _expressionTranslator = expressionTranslator;
            _databaseImplementation = databaseImplementation;
            _logger = logger;
        }

        public Task<long> Insert(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<TEntity> entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token)
        {
            return _repository.Insert(
                transaction,
                entities.Cast<IDatabaseEntity>().ToArray(),
                insertBehavior,
                token);
        }

        public Task<long> Update<TValue>(
            IAdvancedDatabaseTransaction transaction,
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
        {
            return Update(transaction, new[] { new UpdateInfo<TEntity>(Lift(accessor), Lift(valueProducer)) }, predicate, token);

            static Expression<Func<TEntity, object?>> Lift(Expression<Func<TEntity, TValue>> expression)
            {
                return Expression.Lambda<Func<TEntity, object?>>(Expression.Convert(expression.Body, typeof(object)), expression.Parameters);
            }
        }

        public async Task<long> Update(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<UpdateInfo<TEntity>> infos,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            var setExpressions = new List<string>(infos.Count);

            foreach (var info in infos)
            {
                var chain = info
                   .Accessor
                   .ExtractMembersChain()
                   .Select(property => new ColumnProperty(property, property))
                   .ToArray();

                var column = new ColumnInfo(
                    table,
                    chain,
                    _modelProvider);

                if (column.IsMultipleRelation)
                {
                    throw new NotSupportedException($"Unable to update multiple relation: {column.Name}");
                }

                var columnExpression = @$"""{column.Name}""";

                var valueSqlExpression = _expressionTranslator.Translate(info.ValueProducer);

                var valueExpression = InlineQueryParameters(
                    _dependencyContainer,
                    _sqlExpressionTranslator.Translate(valueSqlExpression, 0),
                    valueSqlExpression.ExtractQueryParameters());

                setExpressions.Add(SetExpressionFormat.Format(columnExpression, valueExpression));
            }

            var predicateSqlExpression = _expressionTranslator.Translate(predicate);

            var predicateExpression = InlineQueryParameters(
                _dependencyContainer,
                _sqlExpressionTranslator.Translate(predicateSqlExpression, 0),
                predicateSqlExpression.ExtractQueryParameters());

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Name,
                string.Join(", ", setExpressions),
                predicateExpression);

            var versions = (await transaction
                   .Read<TEntity>()
                   .All()
                   .Where(predicate)
                   .Select(entity => entity.Version)
                   .ToListAsync(token)
                   .ConfigureAwait(false))
               .GroupBy(version => version)
               .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());

            var updateVersion = await transaction
               .GetXid(settings, _logger, token)
               .ConfigureAwait(false);

            var affectedRowsCount = await ExecutionExtensions
               .TryAsync((commandText, settings, _logger), transaction.Execute)
               .Catch<Exception>()
               .Invoke(_databaseImplementation.Handle<long>(commandText), token)
               .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                var change = new UpdateEntityChange<TEntity>(
                    version,
                    count,
                    infos,
                    predicate,
                    updateVersion);

                transaction.CollectChange(change);
            }

            return affectedRowsCount;
        }

        public async Task<long> Delete(
            IAdvancedDatabaseTransaction transaction,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
        {
            // TODO: #178 - add delete behaviors
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            var predicateSqlExpression = _expressionTranslator.Translate(predicate);

            var predicateExpression = InlineQueryParameters(
                _dependencyContainer,
                _sqlExpressionTranslator.Translate(predicateSqlExpression, 0),
                predicateSqlExpression.ExtractQueryParameters());

            var commandText = DeleteValueQueryFormat.Format(
                table.Schema,
                table.Name,
                predicateExpression);

            var versions = (await transaction
                   .Read<TEntity>()
                   .All()
                   .Where(predicate)
                   .Select(entity => entity.Version)
                   .ToListAsync(token)
                   .ConfigureAwait(false))
               .GroupBy(version => version)
               .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());

            var affectedRowsCount = await ExecutionExtensions
               .TryAsync((commandText, settings, _logger), transaction.Execute)
               .Catch<Exception>()
               .Invoke(_databaseImplementation.Handle<long>(commandText), token)
               .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                var change = new DeleteEntityChange<TEntity>(
                    version,
                    count,
                    predicate);

                transaction.CollectChange(change);
            }

            return affectedRowsCount;
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