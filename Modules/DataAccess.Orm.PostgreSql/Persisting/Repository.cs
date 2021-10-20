namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using Settings;
    using Sql.Model;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Scoped)]
    internal class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private const string InsertQueryFormat = @"insert into ""{0}"".""{1}""({2}) values ({3})";
        private const string ColumnFormat = @"""{0}""";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IAdvancedDatabaseTransaction _transaction;

        public Repository(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelProvider modelProvider,
            IAdvancedDatabaseTransaction transaction)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _modelProvider = modelProvider;
            _transaction = transaction;
        }

        public async Task Insert(TEntity entity, CancellationToken token)
        {
            var ormSettings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = entity.GetType();
            var table = _modelProvider.Objects[type.SchemaName()][type.Name];

            var values = table
                .Columns
                .Values
                .Where(column => !column.IsMultipleRelation)
                .ToDictionary(
                    column => column.Name,
                    column =>
                    {
                        var value = column.GetValue<TEntity, TKey>(entity);
                        return value.QueryParameterSqlExpression(_dependencyContainer);
                    },
                    StringComparer.OrdinalIgnoreCase);

            var commandText = InsertQueryFormat.Format(
                table.Schema,
                table.Type.Name,
                values.Keys.Select(column => ColumnFormat.Format(column)).ToString(", "),
                values.Values.ToString(", "));

            var command = new CommandDefinition(
                commandText,
                null,
                _transaction.UnderlyingDbTransaction,
                ormSettings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            await _transaction
                .UnderlyingDbTransaction
                .Connection
                .ExecuteAsync(command)
                .ConfigureAwait(false);
        }

        public Task Update<TValue>(TKey primaryKey, Expression<Func<TEntity, TValue>> accessor, TValue value, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(TKey primaryKey, Expression<Func<TEntity, TValue>> accessor, Expression<Func<TEntity, TValue>> valueProducer, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Delete(TKey primaryKey, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}