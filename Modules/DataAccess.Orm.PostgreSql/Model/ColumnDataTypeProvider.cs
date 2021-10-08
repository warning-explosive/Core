namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Api.Model;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ColumnDataTypeProvider : IColumnDataTypeProvider
    {
        public string GetColumnDataType(Type type)
        {
            if (!type.IsTypeSupported())
            {
                throw new NotSupportedException($"Not supported column type: {type}");
            }

            if (TryGetPrimitiveDataType(type, out var dataType))
            {
                return dataType;
            }

            throw new NotSupportedException($"Not supported column type: {type}");

            static bool TryGetPrimitiveDataType(Type type, [NotNullWhen(true)] out string? dataType)
            {
                type = type.UnwrapTypeParameter(typeof(Nullable<>));

                if (type == typeof(Guid))
                {
                    dataType = EnPostgreSqlDataType.Uuid.ToString();
                    return true;
                }

                if (type == typeof(bool))
                {
                    dataType = EnPostgreSqlDataType.Boolean.ToString();
                    return true;
                }

                if (type == typeof(string))
                {
                    dataType = EnPostgreSqlDataType.Varchar.ToString();
                    return true;
                }

                if (type == typeof(short))
                {
                    dataType = EnPostgreSqlDataType.SmallInt.ToString();
                    return true;
                }

                if (type == typeof(int))
                {
                    dataType = EnPostgreSqlDataType.Integer.ToString();
                    return true;
                }

                if (type == typeof(long))
                {
                    dataType = EnPostgreSqlDataType.BigInt.ToString();
                    return true;
                }

                if (type == typeof(float))
                {
                    // float
                    // ±1.5e−45 to ±3.4e38
                    // ~6-9 digits
                    // 4 bytes
                    dataType = $"{EnPostgreSqlDataType.Numeric}(6, 4)";
                    return true;
                }

                if (type == typeof(double))
                {
                    // double
                    // ±5.0e−324 to ±1.7e308
                    // ~15-17 digits
                    // 8 bytes
                    dataType = $"{EnPostgreSqlDataType.Numeric}(15, 8)";
                    return true;
                }

                if (type == typeof(decimal))
                {
                    // decimal
                    // ±1.0e-28 to ±7.9228e28
                    // 28-29 digits
                    // 16 bytes
                    dataType = $"{EnPostgreSqlDataType.Numeric}(28, 12)";
                    return true;
                }

                if (type == typeof(DateTime))
                {
                    dataType = EnPostgreSqlDataType.Timestamp.ToString();
                    return true;
                }

                if (type == typeof(TimeSpan))
                {
                    dataType = EnPostgreSqlDataType.Interval.ToString();
                    return true;
                }

                dataType = default;
                return false;
            }
        }

        public Type GetColumnType(string dataType)
        {
            if (TryGetPrimitiveType(dataType, out var type))
            {
                return type;
            }

            throw new NotSupportedException($"Not supported column data type: {dataType}");

            static bool TryGetPrimitiveType(string dataType, [NotNullWhen(true)] out Type? type)
            {
                if (dataType.Equals(EnPostgreSqlDataType.Uuid.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(Guid);
                    return true;
                }

                if (dataType.Equals(EnPostgreSqlDataType.Boolean.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(bool);
                    return true;
                }

                if (dataType.Equals(EnPostgreSqlDataType.Varchar.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(string);
                    return true;
                }

                if (dataType.Equals(EnPostgreSqlDataType.SmallInt.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(short);
                    return true;
                }

                if (dataType.Equals(EnPostgreSqlDataType.Integer.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(int);
                    return true;
                }

                if (dataType.Equals(EnPostgreSqlDataType.BigInt.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(long);
                    return true;
                }

                if (dataType.Equals($"{EnPostgreSqlDataType.Numeric}(6, 4)", StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(float);
                    return true;
                }

                if (dataType.Equals($"{EnPostgreSqlDataType.Numeric}(15, 8)", StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(double);
                    return true;
                }

                if (dataType.Equals($"{EnPostgreSqlDataType.Numeric}(28, 12)", StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(decimal);
                    return true;
                }

                if (dataType.Equals(EnPostgreSqlDataType.Timestamp.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(DateTime);
                    return true;
                }

                if (dataType.Equals(EnPostgreSqlDataType.Interval.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(TimeSpan);
                    return true;
                }

                type = default;
                return false;
            }
        }

        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public IEnumerable<string> GetModifiers(CreateColumn createColumn)
        {
            if (createColumn.Type == typeof(Guid)
                && createColumn.Column.Equals(nameof(IUniqueIdentified<Guid>.PrimaryKey), StringComparison.OrdinalIgnoreCase))
            {
                yield return $@"constraint ""{createColumn.Table.ToLowerInvariant()}_pk"" primary key";
            }

            // TODO: #110 - nullable reference
            if (!createColumn.Type.IsNullable()
                || createColumn.Type.IsClass
                || createColumn.Type.IsInterface)
            {
                yield return "not null";
            }
        }
    }
}