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
    using Api.Exceptions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Exceptions;
    using CrossCuttingConcerns.Logging;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using Linq;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using NpgsqlTypes;
    using Orm.Connection;
    using Settings;
    using Sql.Settings;
    using Sql.Translation;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseConnectionProvider : IDatabaseConnectionProvider,
                                                IResolvable<IDatabaseConnectionProvider>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<SqlDatabaseSettings> _sqlSettingsProvider;
        private readonly ISettingsProvider<OrmSettings> _ormSettingsProvider;
        private readonly ILogger _logger;

        public DatabaseConnectionProvider(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<SqlDatabaseSettings> sqlSettingsProvider,
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _sqlSettingsProvider = sqlSettingsProvider;
            _ormSettingsProvider = ormSettingsProvider;
            _logger = logger;
        }

        public string Host => _sqlSettingsProvider.Get(CancellationToken.None).Result.Host;

        public string Database => _sqlSettingsProvider.Get(CancellationToken.None).Result.Database;

        public IsolationLevel IsolationLevel => _sqlSettingsProvider.Get(CancellationToken.None).Result.IsolationLevel;

        public Task<bool> DoesDatabaseExist(CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(DoesDatabaseExistUnsafe)
                .Catch<PostgresException>()
                .Invoke(static (exception, _) => Task.FromResult(!exception.DatabaseDoesNotExist()), token);
        }

        [SuppressMessage("Analysis", "CA2000", Justification = "IDbConnection will be disposed in outer scope by client")]
        public async Task<IDatabaseConnection> OpenConnection(CancellationToken token)
        {
            ValidateNestedCall(_dependencyContainer);

            var settings = await _sqlSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

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

            var npgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

            var connection = new DatabaseConnection(npgSqlConnection);

            await npgSqlConnection
               .OpenAsync(token)
               .ConfigureAwait(false);

            return connection;
        }

        public void Handle(DatabaseCommandExecutionException exception)
        {
            DatabaseException databaseException = exception.Flatten().Any(ex => ex.IsSerializationFailure())
                ? new DatabaseConcurrentUpdateException(exception.CommandText, exception)
                : exception;

            throw databaseException;
        }

        public IAsyncEnumerable<IDictionary<string, object?>> Query(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            return Query(transaction.DbConnection.DbConnection, transaction.DbTransaction, command, token);
        }

        public IAsyncEnumerable<IDictionary<string, object?>> Query(
            IDatabaseConnection connection,
            ICommand command,
            CancellationToken token)
        {
            return Query(connection.DbConnection, default, command, token);
        }

        public Task<long> Execute(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            return Execute(transaction.DbConnection.DbConnection, transaction.DbTransaction, command, token);
        }

        public Task<long> Execute(
            IDatabaseConnection connection,
            ICommand command,
            CancellationToken token)
        {
            return Execute(connection.DbConnection, default, command, token);
        }

        public Task<T> ExecuteScalar<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            return ExecuteScalar<T>(transaction.DbConnection.DbConnection, transaction.DbTransaction, command, token);
        }

        public Task<T> ExecuteScalar<T>(
            IDatabaseConnection connection,
            ICommand command,
            CancellationToken token)
        {
            return ExecuteScalar<T>(connection.DbConnection, default, command, token);
        }

        private async IAsyncEnumerable<IDictionary<string, object?>> Query(
            IDbConnection connection,
            IDbTransaction? transaction,
            ICommand command,
            [EnumeratorCancellation] CancellationToken token)
        {
            var settings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            // TODO: Npgsql doesn't support MARS (Multiple Active Result Sets)
            using (var npgsqlCommand = CreateCommand(connection, transaction, command, settings, _logger))
            {
                using (var reader = await npgsqlCommand.ExecuteReaderAsync(token).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(token).ConfigureAwait(false))
                    {
                        var row = new Dictionary<string, object?>(reader.FieldCount);

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }

                        yield return row;
                    }
                }
            }
        }

        private async Task<long> Execute(
            IDbConnection connection,
            IDbTransaction? transaction,
            ICommand command,
            CancellationToken token)
        {
            var settings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            // TODO: Npgsql doesn't support MARS (Multiple Active Result Sets)
            using (var npgsqlCommand = CreateCommand(connection, transaction, command, settings, _logger))
            {
                return await npgsqlCommand
                    .ExecuteNonQueryAsync(token)
                    .ConfigureAwait(false);
            }
        }

        private async Task<T> ExecuteScalar<T>(
            IDbConnection connection,
            IDbTransaction? transaction,
            ICommand command,
            CancellationToken token)
        {
            var settings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            // TODO: Npgsql doesn't support MARS (Multiple Active Result Sets)
            using (var npgsqlCommand = CreateCommand(connection, transaction, command, settings, _logger))
            {
                var scalar = await npgsqlCommand
                    .ExecuteScalarAsync(token)
                    .ConfigureAwait(false);

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
            ILogger logger)
        {
            if (command is not SqlCommand sqlCommand)
            {
                throw new NotSupportedException($"Unsupported command type {command.GetType()}");
            }

            var npgsqlCommand = new NpgsqlCommand();

            npgsqlCommand.Connection = (NpgsqlConnection)connection;
            npgsqlCommand.Transaction = transaction != null ? (NpgsqlTransaction)transaction : default;
            npgsqlCommand.CommandText = sqlCommand.CommandText;
            npgsqlCommand.CommandTimeout = (int)settings.QuerySecondsTimeout;
            npgsqlCommand.CommandType = CommandType.Text;

            foreach (var (name, value, type) in sqlCommand.CommandParameters)
            {
                if (!TryCast(name, value, out var npgsqlParameter)
                    && !TryInfer(name, value, type, out npgsqlParameter))
                {
                    var typeValue = type == type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>))
                        ? type.Name
                        : type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).Name + "?";

                    throw new NotSupportedException($"Not supported sql command parameter: {name} {value ?? "NULL"}({typeValue})");
                }

                npgsqlCommand.Parameters.Add(npgsqlParameter);
            }

            if (settings.DumpQueries)
            {
                logger.Debug(npgsqlCommand.CommandText);
            }

            return npgsqlCommand;

            static bool TryCast(
                string name,
                object? value,
                [NotNullWhen(true)] out NpgsqlParameter? npgsqlParameter)
            {
                // TODO: handle missing types
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

                // TODO: handle missing types
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
                    var dateTimeValue = value is DateTime dateTime ? dateTime.ToUniversalTime() : default(DateTime?);

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

                npgsqlParameter = null;
                return false;
            }
        }

        private static void ValidateNestedCall(IDependencyContainer dependencyContainer)
        {
            var transaction = ExecutionExtensions
               .Try(dependencyContainer, container => (IAdvancedDatabaseTransaction?)container.Resolve<IAdvancedDatabaseTransaction>())
               .Catch<ComponentResolutionException>()
               .Invoke(_ => default);

            if (transaction?.Connected == true)
            {
                throw new InvalidOperationException("Nested database connections aren't supported");
            }
        }

        private async Task<bool> DoesDatabaseExistUnsafe(CancellationToken token)
        {
            using (await OpenConnection(token).ConfigureAwait(false))
            {
                return true;
            }
        }
    }
}