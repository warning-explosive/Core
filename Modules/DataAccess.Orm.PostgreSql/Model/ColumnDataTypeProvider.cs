 namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;
    using NpgsqlTypes;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Api.Sql.Attributes;
    using SpaceEngineers.Core.DataAccess.Orm.Extensions;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ColumnDataTypeProvider : IColumnDataTypeProvider,
                                            IResolvable<IColumnDataTypeProvider>
    {
        public string GetColumnDataType(ColumnInfo column)
        {
            if (!column.Property.Declared.IsSupportedColumn())
            {
                throw new NotSupportedException($"Unsupported column type: {column.Type}");
            }

            if (TryGetPrimitiveDataType(column, out var dataType))
            {
                return dataType;
            }

            throw new NotSupportedException($"Unsupported column type: {column.Type}");
        }

        private static bool TryGetPrimitiveDataType(ColumnInfo column, [NotNullWhen(true)] out string? dataType)
        {
            var type = column.Type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

            if (type == typeof(short))
            {
                /*
                 * smallint
                 * 2 bytes
                 * -32768 to +32767
                 */
                dataType = NpgsqlDbType.Smallint.ToString();
                return true;
            }

            if (type == typeof(int))
            {
                /*
                 * integer
                 * 4 bytes
                 * -2147483648 to +2147483647
                 */
                dataType = NpgsqlDbType.Integer.ToString();
                return true;
            }

            if (type == typeof(long))
            {
                /*
                 * bigint
                 * 8 bytes
                 * -9223372036854775808 to +9223372036854775807
                 */
                dataType = NpgsqlDbType.Bigint.ToString();
                return true;
            }

            if (type == typeof(float))
            {
                /*
                 * real
                 * 4 bytes
                 * variable-precision, inexact
                 * 6 decimal digits precision
                 *
                 * float
                 * ±1.5e−45 to ±3.4e38
                 * ~6-9 digits
                 * 4 bytes
                 */
                dataType = NpgsqlDbType.Real.ToString();
                return true;
            }

            if (type == typeof(double))
            {
                /*
                 * double precision
                 * 8 bytes
                 * variable-precision, inexact
                 * 15 decimal digits precision
                 *
                 * double
                 * ±5.0e−324 to ±1.7e308
                 * ~15-17 digits
                 * 8 bytes
                 */
                dataType = NpgsqlDbType.Double.ToString();
                return true;
            }

            if (type == typeof(decimal))
            {
                /*
                 * numeric
                 * variable, user-specified precision
                 * up to 131072 digits before the decimal point; up to 16383 digits after the decimal point
                 *
                 * decimal
                 * ±1.0e-28 to ±7.9228e28
                 * 28-29 digits
                 * 16 bytes
                 */
                dataType = $"{NpgsqlDbType.Numeric}(28, 12)";
                return true;
            }

            if (type == typeof(Guid))
            {
                dataType = NpgsqlDbType.Uuid.ToString();
                return true;
            }

            if (type == typeof(bool))
            {
                /*
                 * boolean
                 * 1 byte
                 * state of True or False
                 */
                dataType = NpgsqlDbType.Boolean.ToString();
                return true;
            }

            if (type.IsEnum)
            {
                // TODO: #209 - create ENUM model change;
                // TODO: #209 - cmd.Parameters.Add(new() { Value = "Happy", DataTypeName = "mood" });
                dataType = type.Name;
                return true;
            }

            if (type == typeof(string))
            {
                var columnLenghtAttribute = column.Property.Declared.GetAttribute<ColumnLenghtAttribute>();

                /*
                 * character type
                 * variable-length with limit or with no limit
                 */
                dataType = columnLenghtAttribute == null
                    ? NpgsqlDbType.Text.ToString()
                    : $"{NpgsqlDbType.Varchar}({columnLenghtAttribute.Length})";
                return true;
            }

            if (column.Property.Declared.HasAttribute<JsonColumnAttribute>())
            {
                /*
                 * json stored in a decomposed binary format with indexing support
                 */
                dataType = NpgsqlDbType.Jsonb.ToString();
                return true;
            }

            if (type == typeof(byte[]))
            {
                dataType = NpgsqlDbType.Bytea.ToString();
                return true;
            }

            if (type == typeof(DateTime))
            {
                /*
                 * 4713 BC to 294276 AD
                 * 1 microsecond resolution
                 * with time zone
                 */
                dataType = NpgsqlDbType.TimestampTz.ToString();
                return true;
            }

            if (type == typeof(TimeSpan))
            {
                /*
                 * time interval
                 * 16 bytes
                 * -178000000 years to 178000000 years
                 * 1 microsecond resolution
                 */
                dataType = NpgsqlDbType.Interval.ToString();
                return true;
            }

            if (type == TypeExtensions.FindType("System.Private.CoreLib System.DateOnly"))
            {
                /*
                 * POSGRES:
                 * date (no time of day)
                 * 4 bytes
                 * 4713 BC to 5874897 AD
                 * 1 day resolution
                 *
                 * NET:
                 * 0001-01-01 to 9999-12-31
                 */
                dataType = NpgsqlDbType.Date.ToString();
            }

            if (type == TypeExtensions.FindType("System.Private.CoreLib System.TimeOnly"))
            {
                /*
                 * POSTGRES:
                 * time of day (no date)
                 * 8 bytes
                 * 00:00:00 to 24:00:00
                 * 1 microsecond resolution
                 *
                 * NET:
                 * 00:00:00 to 23:59:59.9999999
                 */
                dataType = NpgsqlDbType.Time.ToString();
            }

            dataType = default;
            return false;
        }
    }
}