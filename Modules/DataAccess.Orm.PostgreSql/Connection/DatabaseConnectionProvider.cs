namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Exceptions;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using Linq;
    using Microsoft.Extensions.Logging;
    using Model;
    using Npgsql;
    using NpgsqlTypes;
    using Orm.Connection;
    using Settings;
    using Sql.Model;
    using Sql.Settings;
    using Sql.Translation;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseConnectionProvider : IDatabaseConnectionProvider,
                                                IResolvable<IDatabaseConnectionProvider>,
                                                IDisposable
    {
        private const string DatabaseExistsCommandText = @"select exists(select * from pg_catalog.pg_database where datname = @param_0);";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<SqlDatabaseSettings> _sqlSettingsProvider;
        private readonly ISettingsProvider<OrmSettings> _ormSettingsProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly NpgsqlDataSource _dataSource;

        public DatabaseConnectionProvider(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<SqlDatabaseSettings> sqlSettingsProvider,
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            IJsonSerializer jsonSerializer,
            IModelProvider modelProvider,
            ILoggerFactory loggerFactory)
        {
            _dependencyContainer = dependencyContainer;
            _sqlSettingsProvider = sqlSettingsProvider;
            _ormSettingsProvider = ormSettingsProvider;
            _jsonSerializer = jsonSerializer;

            var settings = _sqlSettingsProvider.Get(CancellationToken.None).Result;

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = settings.Host,
                Port = settings.Port,
                Database = settings.Database,
                Username = settings.Username,
                Password = settings.Password,
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = (int)settings.ConnectionPoolSize,
                ConnectionPruningInterval = 1,
                ConnectionIdleLifetime = 1,
                IncludeErrorDetail = true
            };

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionStringBuilder.ConnectionString)
                .EnableParameterLogging()
                .UseLoggerFactory(loggerFactory);

            foreach (var info in modelProvider.Enums)
            {
                dataSourceBuilder
                    .CallMethod(nameof(NpgsqlDataSourceBuilder.MapEnum))
                    .WithTypeArguments(info.Type)
                    .WithArguments(info.Name, new NpgsqlEnumNameTranslator())
                    .Invoke();
            }

            _dataSource = dataSourceBuilder.Build();
        }

        public string Host => _sqlSettingsProvider.Get(CancellationToken.None).Result.Host;

        public string Database => _sqlSettingsProvider.Get(CancellationToken.None).Result.Database;

        public IsolationLevel IsolationLevel => _sqlSettingsProvider.Get(CancellationToken.None).Result.IsolationLevel;

        public void Dispose()
        {
            _dataSource.Dispose();
        }

        public Task<bool> DoesDatabaseExist(CancellationToken token)
        {
            return DoesDatabaseExistUnsafe(token)
                .TryAsync()
                .Catch<Exception>()
                .Invoke(static exception => exception.Flatten().All(ex => !ex.DatabaseDoesNotExist()), token);
        }

        public async Task<IDbConnection> OpenConnection(CancellationToken token)
        {
            ValidateNestedCall(_dependencyContainer);

            return await _dataSource
                .OpenConnectionAsync(token)
                .ConfigureAwait(false);
        }

        public Task<long> Execute(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            return Execute(transaction.DbConnection, transaction, command, token);
        }

        public Task<long> Execute(
            IAdvancedDatabaseTransaction transaction,
            IEnumerable<ICommand> commands,
            CancellationToken token)
        {
            return Execute(transaction.DbConnection, transaction, commands, token);
        }

        public Task<long> Execute(
            IDbConnection connection,
            ICommand command,
            CancellationToken token)
        {
            return Execute(connection, default, command, token);
        }

        public Task<long> Execute(
            IDbConnection connection,
            IEnumerable<ICommand> commands,
            CancellationToken token)
        {
            return Execute(connection, default, commands, token);
        }

        public IAsyncEnumerable<IDictionary<string, object?>> Query(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            return Query(transaction.DbConnection, transaction, command, token);
        }

        public IAsyncEnumerable<IDictionary<string, object?>> Query(
            IDbConnection connection,
            ICommand command,
            CancellationToken token)
        {
            return Query(connection, default, command, token);
        }

        public Task<T> ExecuteScalar<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            return ExecuteScalar<T>(transaction.DbConnection, transaction, command, token);
        }

        public Task<T> ExecuteScalar<T>(
            IDbConnection connection,
            ICommand command,
            CancellationToken token)
        {
            return ExecuteScalar<T>(connection, default, command, token);
        }

        private async Task<long> Execute(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            IEnumerable<ICommand> commands,
            CancellationToken token)
        {
            var settings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            long result = 0;

            foreach (var command in commands)
            {
                using (var npgsqlCommand = CreateCommand(connection, transaction?.DbTransaction, command, settings, _jsonSerializer))
                {
                    result += await npgsqlCommand
                        .ExecuteNonQueryAsync(token)
                        .Handled(command)
                        .ConfigureAwait(false);
                }

                if (command is SqlCommand { Collect: true })
                {
                    transaction?.CollectCommand(command);
                }
            }

            return result;
        }

        private async Task<long> Execute(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            ICommand command,
            CancellationToken token)
        {
            var settings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            using (var npgsqlCommand = CreateCommand(connection, transaction?.DbTransaction, command, settings, _jsonSerializer))
            {
                var affectedRowsCount = await npgsqlCommand
                    .ExecuteNonQueryAsync(token)
                    .Handled(command)
                    .ConfigureAwait(false);

                if (command is SqlCommand { Collect: true })
                {
                    transaction?.CollectCommand(command);
                }

                return affectedRowsCount;
            }
        }

        private async IAsyncEnumerable<IDictionary<string, object?>> Query(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            ICommand command,
            [EnumeratorCancellation] CancellationToken token)
        {
            var settings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            using (var npgsqlCommand = CreateCommand(connection, transaction?.DbTransaction, command, settings, _jsonSerializer))
            {
                var reader = await npgsqlCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess, token)
                    .Handled(command)
                    .ConfigureAwait(false);

                /*
                 * npgsql doesn't support MARS (Multiple Active Result Sets)
                 * https://github.com/npgsql/npgsql/issues/462#issuecomment-756787766
                 * https://github.com/npgsql/npgsql/issues/3990
                 */
                var buffer = new List<IDictionary<string, object?>>();

                await using (reader)
                {
                    while (await reader.ReadAsync(token).Handled(command).ConfigureAwait(false))
                    {
                        var row = new Dictionary<string, object?>(reader.FieldCount);

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }

                        buffer.Add(row);
                    }

                    while (await reader.NextResultAsync(token).Handled(command).ConfigureAwait(false))
                    {
                        /* ignore subsequent result sets */
                    }
                }

                if (command is SqlCommand { Collect: true })
                {
                    transaction?.CollectCommand(command);
                }

                foreach (var row in buffer)
                {
                    yield return row;
                }
            }
        }

        private async Task<T> ExecuteScalar<T>(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            ICommand command,
            CancellationToken token)
        {
            var settings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            using (var npgsqlCommand = CreateCommand(connection, transaction?.DbTransaction, command, settings, _jsonSerializer))
            {
                var scalar = await npgsqlCommand
                    .ExecuteScalarAsync(token)
                    .Handled(command)
                    .ConfigureAwait(false);

                if (command is SqlCommand { Collect: true })
                {
                    transaction?.CollectCommand(command);
                }

                return (T)scalar !;
            }
        }

        [SuppressMessage("Analysis", "CA2100", Justification = "NpgsqlParameter<T>")]
        [SuppressMessage("Analysis", "CA1502", Justification = "NpgsqlParameter<T>")]
        private static NpgsqlCommand CreateCommand(
            IDbConnection connection,
            IDbTransaction? transaction,
            ICommand command,
            OrmSettings settings,
            IJsonSerializer jsonSerializer)
        {
            if (command is not SqlCommand sqlCommand)
            {
                throw new NotSupportedException($"Unsupported command type {command.GetType()}");
            }

            /*
             * TODO: #209 - Batch support
             * https://github.com/dotnet/runtime/issues/28633
             */
            var npgsqlCommand = new NpgsqlCommand();

            npgsqlCommand.Connection = (NpgsqlConnection)connection;
            npgsqlCommand.Transaction = (NpgsqlTransaction?)transaction;
            npgsqlCommand.CommandText = sqlCommand.CommandText;
            npgsqlCommand.CommandTimeout = (int)settings.CommandSecondsTimeout;
            npgsqlCommand.CommandType = CommandType.Text;

            foreach (var parameter in sqlCommand.CommandParameters)
            {
                var (name, value, type, isJsonValue) = parameter;

                if (!TryCast(name, value, out var npgsqlParameter)
                    && !TryInfer(name, value, type, out npgsqlParameter)
                    && !TrySerialize(name, value, type, isJsonValue, jsonSerializer, out npgsqlParameter))
                {
                    throw new NotSupportedException($"Not supported sql command parameter: {parameter}");
                }

                npgsqlCommand.Parameters.Add(npgsqlParameter);
            }

            return npgsqlCommand;

            static bool TryCast(
                string name,
                object? value,
                [NotNullWhen(true)] out NpgsqlParameter? npgsqlParameter)
            {
                npgsqlParameter = value switch
                {
                    short @short => new NpgsqlParameter<short>(name, NpgsqlDbType.Smallint) { TypedValue = @short },
                    int @int => new NpgsqlParameter<int>(name, NpgsqlDbType.Integer) { TypedValue = @int },
                    long @long => new NpgsqlParameter<long>(name, NpgsqlDbType.Bigint) { TypedValue = @long },
                    float @float => new NpgsqlParameter<float>(name, NpgsqlDbType.Real) { TypedValue = @float },
                    double @double => new NpgsqlParameter<double>(name, NpgsqlDbType.Double) { TypedValue = @double },
                    decimal @decimal => new NpgsqlParameter<decimal>(name, NpgsqlDbType.Numeric) { TypedValue = @decimal },
                    Guid guid => new NpgsqlParameter<Guid>(name, NpgsqlDbType.Uuid) { TypedValue = guid },
                    bool @bool => new NpgsqlParameter<bool>(name, NpgsqlDbType.Boolean) { TypedValue = @bool },
                    string @string => new NpgsqlParameter<string>(name, NpgsqlDbType.Varchar) { TypedValue = @string },
                    byte[] byteArray => new NpgsqlParameter<byte[]>(name, NpgsqlDbType.Bytea) { TypedValue = byteArray },
                    DateTime dateTime => new NpgsqlParameter<DateTime>(name, NpgsqlDbType.TimestampTz) { TypedValue = dateTime.ToUniversalTime() },
                    TimeSpan timeSpan => new NpgsqlParameter<TimeSpan>(name, NpgsqlDbType.Interval) { TypedValue = timeSpan },
                    _ => null
                };

                return npgsqlParameter != null;
            }

            static bool TryInfer(
                string name,
                object? value,
                Type type,
                [NotNullWhen(true)] out NpgsqlParameter? npgsqlParameter)
            {
                var nullType = type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

                if (nullType == typeof(short))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Smallint) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(int))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Integer) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(long))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Bigint) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(float))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Real) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(double))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Double) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(decimal))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Numeric) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(Guid))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Uuid) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(bool))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Boolean) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(string))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Varchar) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(byte[]))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Bytea) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == typeof(DateTime))
                {
                    object dateTimeValue = value is DateTime dateTime ? dateTime.ToUniversalTime() : DBNull.Value;

                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.TimestampTz) { Value = dateTimeValue };
                    return true;
                }

                if (nullType == typeof(TimeSpan))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Interval) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == TypeExtensions.FindType("System.Private.CoreLib System.DateOnly"))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Date) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == TypeExtensions.FindType("System.Private.CoreLib System.TimeOnly"))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Time) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType.IsEnum)
                {
                    npgsqlParameter = new NpgsqlParameter(name, value ?? DBNull.Value) { DataTypeName = nullType.Name };
                    return true;
                }

                if (nullType.IsCollection()
                    && TryInfer(name, null, nullType.ExtractGenericArgumentAt(typeof(IEnumerable<>)), out var itemNpgsqlParameter))
                {
                    npgsqlParameter = itemNpgsqlParameter.DataTypeName == null
                        ? new NpgsqlParameter(name, NpgsqlDbType.Array | itemNpgsqlParameter.NpgsqlDbType) { Value = value ?? DBNull.Value }
                        : new NpgsqlParameter(name, value ?? DBNull.Value) { DataTypeName = itemNpgsqlParameter.DataTypeName + "[]" };

                    return true;
                }

                npgsqlParameter = null;
                return false;
            }

            static bool TrySerialize(
                string name,
                object? value,
                Type type,
                bool isJsonValue,
                IJsonSerializer jsonSerializer,
                [NotNullWhen(true)] out NpgsqlParameter? npgsqlParameter)
            {
                if (isJsonValue)
                {
                    object jsonValue = value != null
                        ? jsonSerializer.SerializeObject(value, type)
                        : DBNull.Value;

                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Jsonb) { Value = jsonValue };
                    return true;
                }

                npgsqlParameter = null;
                return false;
            }
        }

        private static void ValidateNestedCall(IDependencyContainer dependencyContainer)
        {
            var transaction = ExecutionExtensions
               .Try(container => (IAdvancedDatabaseTransaction?)container.Resolve<IAdvancedDatabaseTransaction>(), dependencyContainer)
               .Catch<ComponentResolutionException>()
               .Invoke(_ => default);

            if (transaction?.Connected == true)
            {
                throw new InvalidOperationException("Nested database connections aren't supported");
            }
        }

        private Task<bool> DoesDatabaseExistUnsafe(CancellationToken token)
        {
            var command = new SqlCommand(
                DatabaseExistsCommandText,
                new List<SqlCommandParameter> { new SqlCommandParameter("param_0", Database, typeof(string)) });

            return _dependencyContainer.InvokeWithinTransaction(false, command, ExecuteScalar<bool>, token);
        }
    }
}