namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Connection
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CrossCuttingConcerns.Json;
    using Linq;
    using Microsoft.Extensions.Logging;
    using Model;
    using Npgsql;
    using NpgsqlTypes;
    using Settings;
    using SpaceEngineers.Core.CompositionRoot.Exceptions;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Connection;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseConnectionProvider : IDatabaseConnectionProvider,
                                                IResolvable<IDatabaseConnectionProvider>,
                                                IDisposable
    {
        private const string DatabaseExistsCommandText = @"select exists(select * from pg_catalog.pg_database where datname = @param_0);";
        private const string TransactionIdCommandText = "select txid_current()";

        private static ConcurrentDictionary<Type, ConcurrentDictionary<int, NullableArrayInfo>> _nullableArrayCache
            = new ConcurrentDictionary<Type, ConcurrentDictionary<int, NullableArrayInfo>>();

        private static MethodInfo _getFieldValueGenericMethod = new MethodFinder(
                typeof(NpgsqlDataReader),
                nameof(NpgsqlDataReader.GetFieldValue),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
            {
                TypeArguments = new[] { typeof(object) },
                ArgumentTypes = new[] { typeof(int) }
            }.FindMethod() ?? throw new InvalidOperationException($"Could not find {nameof(NpgsqlDataReader)}.{nameof(NpgsqlDataReader.GetFieldValue)}() method");

        private readonly SqlDatabaseSettings _sqlDatabaseSettings;
        private readonly OrmSettings _ormSettings;
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly NpgsqlDataSource _dataSource;

        public DatabaseConnectionProvider(
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            IDependencyContainer dependencyContainer,
            IJsonSerializer jsonSerializer,
            IModelProvider modelProvider,
            ILoggerFactory loggerFactory)
        {
            _sqlDatabaseSettings = sqlDatabaseSettingsProvider.Get();
            _ormSettings = ormSettingsProvider.Get();

            _dependencyContainer = dependencyContainer;
            _jsonSerializer = jsonSerializer;

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                ApplicationName = _sqlDatabaseSettings.ApplicationName,
                Host = _sqlDatabaseSettings.Host,
                Port = _sqlDatabaseSettings.Port,
                Database = _sqlDatabaseSettings.Database,
                Username = _sqlDatabaseSettings.Username,
                Password = _sqlDatabaseSettings.Password,
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = (int)_sqlDatabaseSettings.ConnectionPoolSize,
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

        public Task<long> GetVersion(IAdvancedDatabaseTransaction transaction, CancellationToken token)
        {
            return ExecuteScalar<long>(
                transaction,
                new SqlCommand(TransactionIdCommandText, Array.Empty<SqlCommandParameter>(), false),
                token);
        }

        public async ValueTask<IDbConnection> OpenConnection(CancellationToken token)
        {
            ValidateNestedCall(_dependencyContainer);

            return await _dataSource
                .OpenConnectionAsync(token)
                .ConfigureAwait(false);
        }

        public async ValueTask<IDbTransaction> BeginTransaction(IDbConnection connection, CancellationToken token)
        {
            return await ((NpgsqlConnection)connection)
                .BeginTransactionAsync(_sqlDatabaseSettings.IsolationLevel, token)
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

        public IAsyncEnumerable<IDictionary<string, object?>> Query<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            return Query<T>(transaction.DbConnection, transaction, command, token);
        }

        public IAsyncEnumerable<IDictionary<string, object?>> Query<T>(
            IDbConnection connection,
            ICommand command,
            CancellationToken token)
        {
            return Query<T>(connection, default, command, token);
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
            long result = 0;

            using (var npgsqlBatch = CreateBatch(connection, transaction, commands, _ormSettings))
            {
                result += await npgsqlBatch
                    .ExecuteNonQueryAsync(token)
                    .Handled(npgsqlBatch)
                    .ConfigureAwait(false);
            }

            return result;
        }

        private async Task<long> Execute(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            ICommand command,
            CancellationToken token)
        {
            using (var npgsqlCommand = CreateCommand(connection, transaction, command, _ormSettings))
            {
                return await npgsqlCommand
                    .ExecuteNonQueryAsync(token)
                    .Handled(npgsqlCommand)
                    .ConfigureAwait(false);
            }
        }

        private async IAsyncEnumerable<IDictionary<string, object?>> Query<T>(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            ICommand command,
            [EnumeratorCancellation] CancellationToken token)
        {
            /*
             * npgsql doesn't support MARS (Multiple Active Result Sets)
             * https://github.com/npgsql/npgsql/issues/462#issuecomment-756787766
             * https://github.com/npgsql/npgsql/issues/3990
             */
            var buffer = new List<IDictionary<string, object?>>();

            var type = typeof(T);

            using (var npgsqlCommand = CreateCommand(connection, transaction, command, _ormSettings))
            {
                var reader = await npgsqlCommand
                    .ExecuteReaderAsync(CommandBehavior.SequentialAccess, token)
                    .Handled(npgsqlCommand)
                    .ConfigureAwait(false);

                await using (reader)
                {
                    while (await reader.ReadAsync(token).Handled(npgsqlCommand).ConfigureAwait(false))
                    {
                        var row = new Dictionary<string, object?>(reader.FieldCount);

                        for (var ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                        {
                            var name = reader.GetName(ordinal);

                            var nullableArrayInfo = _nullableArrayCache
                                .GetOrAdd(
                                    type,
                                    static (_, _) => new ConcurrentDictionary<int, NullableArrayInfo>(),
                                    ordinal)
                                .GetOrAdd(
                                    ordinal,
                                    static (_, state) => new NullableArrayInfo(state.reader, state.ordinal, state.name, state.type),
                                    (reader, ordinal, name, type));

                            if (nullableArrayInfo.IsNullableArray(out var getFieldValueMethod))
                            {
                                row[name] = getFieldValueMethod.Invoke(reader, new object?[] { ordinal });
                            }
                            else
                            {
                                row[name] = reader.GetValue(ordinal);
                            }
                        }

                        buffer.Add(row);
                    }

                    while (await reader.NextResultAsync(token).Handled(npgsqlCommand).ConfigureAwait(false))
                    {
                        /* ignore subsequent result sets */
                    }
                }
            }

            foreach (var row in buffer)
            {
                yield return row;
            }
        }

        private async Task<T> ExecuteScalar<T>(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            ICommand command,
            CancellationToken token)
        {
            using (var npgsqlCommand = CreateCommand(connection, transaction, command, _ormSettings))
            {
                var scalar = await npgsqlCommand
                    .ExecuteScalarAsync(token)
                    .Handled(npgsqlCommand)
                    .ConfigureAwait(false);

                return (T)scalar !;
            }
        }

        private NpgsqlBatch CreateBatch(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            IEnumerable<ICommand> commands,
            OrmSettings settings)
        {
            // TODO: #backlog - batch support doesn't work in netstandard assembly - https://github.com/dotnet/runtime/issues/28633
            var npgsqlBatch = new NpgsqlBatch();

            npgsqlBatch.Connection = (NpgsqlConnection)connection;
            npgsqlBatch.Transaction = (NpgsqlTransaction?)transaction?.DbTransaction;
            npgsqlBatch.Timeout = (int)settings.CommandSecondsTimeout;

            foreach (var command in commands)
            {
                if (command is not SqlCommand sqlCommand)
                {
                    throw new NotSupportedException($"Unsupported command type {command.GetType()}");
                }

                var npgsqlCommand = new NpgsqlBatchCommand();
                npgsqlBatch.BatchCommands.Add(npgsqlCommand);

                npgsqlCommand.CommandText = sqlCommand.CommandText;
                npgsqlCommand.CommandType = CommandType.Text;

                foreach (var parameter in sqlCommand.CommandParameters)
                {
                    npgsqlCommand.Parameters.Add(GetNpgsqlParameter(parameter));
                }

                if (sqlCommand.Collect)
                {
                    transaction?.CollectCommand(command);
                }
            }

            return npgsqlBatch;
        }

        [SuppressMessage("Analysis", "CA2100", Justification = "NpgsqlParameter<T>")]
        private NpgsqlCommand CreateCommand(
            IDbConnection connection,
            IAdvancedDatabaseTransaction? transaction,
            ICommand command,
            OrmSettings settings)
        {
            if (command is not SqlCommand sqlCommand)
            {
                throw new NotSupportedException($"Unsupported command type {command.GetType()}");
            }

            var npgsqlCommand = new NpgsqlCommand();

            npgsqlCommand.Connection = (NpgsqlConnection)connection;
            npgsqlCommand.Transaction = (NpgsqlTransaction?)transaction?.DbTransaction;
            npgsqlCommand.CommandTimeout = (int)settings.CommandSecondsTimeout;
            npgsqlCommand.CommandText = sqlCommand.CommandText;
            npgsqlCommand.CommandType = CommandType.Text;

            foreach (var parameter in sqlCommand.CommandParameters)
            {
                npgsqlCommand.Parameters.Add(GetNpgsqlParameter(parameter));
            }

            if (sqlCommand.Collect)
            {
                transaction?.CollectCommand(command);
            }

            return npgsqlCommand;
        }

        [SuppressMessage("Analysis", "CA1502", Justification = "complex infrastructural code")]
        private NpgsqlParameter GetNpgsqlParameter(SqlCommandParameter parameter)
        {
            var (name, value, type) = parameter;

            if (!TryCast(name, value, _jsonSerializer, out var npgsqlParameter)
                && !TryInfer(name, value, type, _jsonSerializer, out npgsqlParameter))
            {
                throw new NotSupportedException($"Not supported sql command parameter: {parameter}");
            }

            return npgsqlParameter;

            static bool TryCast(
                string name,
                object? value,
                IJsonSerializer jsonSerializer,
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
                    DatabaseJsonObject jsonObject => new NpgsqlParameter(name, NpgsqlDbType.Jsonb) { Value = jsonObject.Value != null ? jsonSerializer.SerializeObject(jsonObject.Value, jsonObject.Type) : DBNull.Value },
                    _ => null
                };

                return npgsqlParameter != null;
            }

            static bool TryInfer(
                string name,
                object? value,
                Type type,
                IJsonSerializer jsonSerializer,
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

                if (nullType == Basics.TypeExtensions.FindType("System.Private.CoreLib System.DateOnly"))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Date) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType == Basics.TypeExtensions.FindType("System.Private.CoreLib System.TimeOnly"))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Time) { Value = value ?? DBNull.Value };
                    return true;
                }

                if (nullType.IsEnum)
                {
                    string enumDataTypeName;
                    object? enumValue;

                    if (nullType.IsEnumFlags())
                    {
                        enumValue = value is Enum @enum
                            ? @enum.EnumFlagsValues()
                            : null;

                        enumDataTypeName = nullType.Name + "[]";
                    }
                    else
                    {
                        enumValue = value;
                        enumDataTypeName = nullType.Name;
                    }

                    npgsqlParameter = new NpgsqlParameter(name, enumValue ?? DBNull.Value) { DataTypeName = enumDataTypeName };
                    return true;
                }

                if (typeof(DatabaseJsonObject).IsAssignableFrom(type))
                {
                    npgsqlParameter = new NpgsqlParameter(name, NpgsqlDbType.Jsonb) { Value = value is DatabaseJsonObject jsonObject && jsonObject.Value != null ? jsonSerializer.SerializeObject(jsonObject.Value, jsonObject.Type) : DBNull.Value };
                    return true;
                }

                if (nullType.IsCollection()
                    && TryInfer(name, null, nullType.ExtractGenericArgumentAt(typeof(IEnumerable<>)), jsonSerializer, out var itemNpgsqlParameter))
                {
                    npgsqlParameter = itemNpgsqlParameter.DataTypeName == null
                        ? new NpgsqlParameter(name, NpgsqlDbType.Array | itemNpgsqlParameter.NpgsqlDbType) { Value = value ?? DBNull.Value }
                        : new NpgsqlParameter(name, value ?? DBNull.Value) { DataTypeName = itemNpgsqlParameter.DataTypeName + "[]" };

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

        private async Task<bool> DoesDatabaseExistUnsafe(CancellationToken token)
        {
            var command = new SqlCommand(
                DatabaseExistsCommandText,
                new List<SqlCommandParameter> { new SqlCommandParameter("param_0", _sqlDatabaseSettings.Database, typeof(string)) });

            return await _dependencyContainer
                .InvokeWithinTransaction(false, command, ExecuteScalar<bool>, token)
                .ConfigureAwait(false);
        }

        private class NullableArrayInfo
        {
            private readonly MethodInfo? _getFieldValueMethod;

            public NullableArrayInfo(
                NpgsqlDataReader reader,
                int ordinal,
                string propertyName,
                Type targetType)
            {
                var sourceType = reader.GetFieldType(ordinal);

                _getFieldValueMethod = sourceType.IsArray()
                                       && (sourceType.IsDatabaseArray(out var elementType) || targetType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance).PropertyType.IsDatabaseArray(out elementType))
                                       && elementType != null
                                       && elementType.IsNullable()
                    ? _getFieldValueGenericMethod.MakeGenericMethod(elementType.MakeArrayType())
                    : null;
            }

            public bool IsNullableArray([NotNullWhen(true)] out MethodInfo? getFieldValueMethod)
            {
                getFieldValueMethod = _getFieldValueMethod;
                return getFieldValueMethod != null;
            }
        }
    }
}