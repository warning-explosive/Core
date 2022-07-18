namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Extensions;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ColumnDataTypeProvider : IColumnDataTypeProvider,
                                            IResolvable<IColumnDataTypeProvider>
    {
        public string GetColumnDataType(ColumnInfo columnInfo)
        {
            if (!columnInfo.Type.IsTypeSupported())
            {
                throw new NotSupportedException($"Not supported column type: {columnInfo.Type}");
            }

            if (TryGetPrimitiveDataType(columnInfo.Type, out var dataType))
            {
                return dataType;
            }

            throw new NotSupportedException($"Not supported column type: {columnInfo.Type}");

            static bool TryGetPrimitiveDataType(Type type, [NotNullWhen(true)] out string? dataType)
            {
                type = type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

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
    }
}