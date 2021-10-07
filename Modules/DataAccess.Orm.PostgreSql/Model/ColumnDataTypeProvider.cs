namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Collections.Generic;
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

            type = type.UnwrapTypeParameter(typeof(Nullable<>));

            if (type == typeof(Guid))
            {
                return EnPostgreSqlDataType.Uuid.ToString();
            }

            if (type == typeof(bool))
            {
                return EnPostgreSqlDataType.Boolean.ToString();
            }

            if (type == typeof(string))
            {
                return EnPostgreSqlDataType.Varchar.ToString();
            }

            if (type == typeof(short))
            {
                return EnPostgreSqlDataType.SmallInt.ToString();
            }

            if (type == typeof(int))
            {
                return EnPostgreSqlDataType.Integer.ToString();
            }

            if (type == typeof(long))
            {
                return EnPostgreSqlDataType.BigInt.ToString();
            }

            if (type == typeof(float))
            {
                // float
                // ±1.5e−45 to ±3.4e38
                // ~6-9 digits
                // 4 bytes
                return $"{EnPostgreSqlDataType.Numeric}(6, 4)";
            }

            if (type == typeof(double))
            {
                // double
                // ±5.0e−324 to ±1.7e308
                // ~15-17 digits
                // 8 bytes
                return $"{EnPostgreSqlDataType.Numeric}(15, 8)";
            }

            if (type == typeof(decimal))
            {
                // decimal
                // ±1.0e-28 to ±7.9228e28
                // 28-29 digits
                // 16 bytes
                return $"{EnPostgreSqlDataType.Numeric}(28, 12)";
            }

            if (type == typeof(DateTime))
            {
                return EnPostgreSqlDataType.Timestamp.ToString();
            }

            if (type == typeof(TimeSpan))
            {
                return EnPostgreSqlDataType.Interval.ToString();
            }

            // TODO: #110 - relations
            /*if (type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                return;
            }

            if (type.IsSubclassOfOpenGeneric(typeof(IInlinedObject)))
            {
                return;
            }

            if (type.IsSubclassOfOpenGeneric(typeof(IReadOnlyCollection<>)))
            {
                return;
            }

            if (type.IsSubclassOfOpenGeneric(typeof(ICollection<>)))
            {
                return;
            }*/

            throw new NotSupportedException($"Not supported column type: {type}");
        }

        public Type GetColumnType(string dataType)
        {
            if (dataType.Equals(EnPostgreSqlDataType.Uuid.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return typeof(Guid);
            }

            if (dataType.Equals(EnPostgreSqlDataType.Boolean.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return typeof(bool);
            }

            if (dataType.Equals(EnPostgreSqlDataType.Varchar.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return typeof(string);
            }

            if (dataType.Equals(EnPostgreSqlDataType.SmallInt.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return typeof(short);
            }

            if (dataType.Equals(EnPostgreSqlDataType.Integer.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return typeof(int);
            }

            if (dataType.Equals(EnPostgreSqlDataType.BigInt.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return typeof(long);
            }

            if (dataType.Equals($"{EnPostgreSqlDataType.Numeric}(6, 4)", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(float);
            }

            if (dataType.Equals($"{EnPostgreSqlDataType.Numeric}(15, 8)", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(double);
            }

            if (dataType.Equals($"{EnPostgreSqlDataType.Numeric}(28, 12)", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(decimal);
            }

            if (dataType.Equals(EnPostgreSqlDataType.Timestamp.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return typeof(DateTime);
            }

            if (dataType.Equals(EnPostgreSqlDataType.Interval.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return typeof(TimeSpan);
            }

            // TODO: #110 - relations
            /*if (type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                return;
            }

            if (type.IsSubclassOfOpenGeneric(typeof(IInlinedObject)))
            {
                return;
            }

            if (type.IsSubclassOfOpenGeneric(typeof(IReadOnlyCollection<>)))
            {
                return;
            }

            if (type.IsSubclassOfOpenGeneric(typeof(ICollection<>)))
            {
                return;
            }*/

            throw new NotSupportedException($"Not supported column data type: {dataType}");
        }

        public IEnumerable<string> GetModifiers(Type type)
        {
            // TODO: #110 - nullable reference
            if (!type.IsNullable()
                || type.IsClass
                || type.IsInterface)
            {
                yield return "not null";
            }
        }
    }
}