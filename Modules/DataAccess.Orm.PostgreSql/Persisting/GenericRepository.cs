namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
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

    [Component(EnLifestyle.Scoped)]
    internal class GenericRepository<TEntity, TKey> : IRepository<TEntity, TKey>,
                                                      IResolvable<IRepository<TEntity, TKey>>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private const string UpdateValueQueryFormat = @"update ""{0}"".""{1}"" set {2} where ""PrimaryKey"" in {3}";
        private const string DeleteValueQueryFormat = @"delete ""{0}"".""{1}"" where ""PrimaryKey"" in {2}";

        private const string SetExpressionFormat = @"{0} = {1}";
        private const string ColumnFormat = @"""{0}""";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly IRepository _repository;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IDatabaseTransaction _transaction;
        private readonly IExpressionTranslator _expressionTranslator;

        public GenericRepository(
            IDependencyContainer dependencyContainer,
            IRepository repository,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelProvider modelProvider,
            IDatabaseTransaction transaction,
            IExpressionTranslator expressionTranslator)
        {
            _dependencyContainer = dependencyContainer;
            _repository = repository;
            _settingsProvider = settingsProvider;
            _modelProvider = modelProvider;
            _transaction = transaction;
            _expressionTranslator = expressionTranslator;
        }

        public Task Insert(
            TEntity[] entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token)
        {
            return _repository.Insert(
                entities.Cast<IUniqueIdentified>().ToArray(),
                insertBehavior,
                token);
        }

        public async Task Update<TValue>(
            TKey[] primaryKeys,
            Expression<Func<TEntity, TValue>> accessor,
            TValue value,
            CancellationToken token)
        {
            if (!primaryKeys.Any())
            {
                return;
            }

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

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Name,
                SetExpressionFormat.Format(ColumnFormat.Format(column.Name), value.QueryParameterSqlExpression(_dependencyContainer)),
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            try
            {
                _ = await _transaction
                    .UnderlyingDbTransaction
                    .InvokeScalar(commandText, settings, token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }
        }

        public async Task Update<TValue>(
            TKey[] primaryKeys,
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            CancellationToken token)
        {
            if (!primaryKeys.Any())
            {
                return;
            }

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

            var valueExpression = _expressionTranslator
                .Translate(valueProducer)
                .Translate(_dependencyContainer, 0);

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Name,
                SetExpressionFormat.Format(ColumnFormat.Format(column.Name), valueExpression),
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            try
            {
                _ = await _transaction
                    .UnderlyingDbTransaction
                    .Invoke(commandText, settings, token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }
        }

        public async Task Delete(
            TKey[] primaryKeys,
            CancellationToken token)
        {
            if (!primaryKeys.Any())
            {
                return;
            }

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            var commandText = DeleteValueQueryFormat.Format(
                table.Type,
                table.Name,
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            try
            {
                _ = await _transaction
                    .UnderlyingDbTransaction
                    .Invoke(commandText, settings, token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }
        }
    }
}