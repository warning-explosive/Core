namespace SpaceEngineers.Core.DataImport
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using Abstractions;
    using Basics;

    /// <inheritdoc />
    public abstract class DataTableReaderBase<TElement, TTableMeta> : IDataTableReader<TElement, TTableMeta>
        where TTableMeta : IDataTableMeta
    {
        /// <inheritdoc />
        public abstract IReadOnlyDictionary<string, string> PropertyToColumnCaption { get; }

        private Func<string?, IFormatProvider, int?> IntegerParser { get; }
            = (value, formatter)
                => int.TryParse(value, NumberStyles.Any, formatter, out var number)
                    ? number
                    : null;

        private Func<string?, IFormatProvider, decimal?> DecimalParser { get; }
            = (value, formatter)
                => decimal.TryParse(value, NumberStyles.Any, formatter, out var number)
                    ? number
                    : null;

        private Func<string?, IFormatProvider, DateTime?> DateTimeParser { get; }
            = (value, formatter)
                => DateTime.TryParse(value, formatter, DateTimeStyles.AllowWhiteSpaces, out var dateTime)
                    ? dateTime
                    : null;

        /// <inheritdoc />
        public abstract TElement? ReadRow(
            DataRow row,
            int rowIndex,
            IReadOnlyDictionary<string, string> propertyToColumn,
            TTableMeta tableMeta);

        /// <inheritdoc />
        public abstract void AfterTableRead();

        /// <summary>
        /// DataRow is empty (each column has no value)
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>DataRow is empty attribute</returns>
        protected bool RowIsEmpty(
            DataRow row,
            IReadOnlyDictionary<string, string> propertyToColumn)
        {
            return propertyToColumn.All(pair => row[pair.Value].ToString().IsNullOrEmpty());
        }

        /// <summary>
        /// DataRow is fully filled (each column has value)
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>DataRow is fully filled attribute</returns>
        protected bool RowIsFullyFilled(
            DataRow row,
            IReadOnlyDictionary<string, string> propertyToColumn)
        {
            return propertyToColumn.All(pair => !row[pair.Value].ToString().IsNullOrEmpty());
        }

        /// <summary>
        /// Read property value as object
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>Object-value</returns>
        protected object? Read(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn)
        {
            return row[propertyToColumn[property]];
        }

        /// <summary>
        /// Read property value as nullable string
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>string-value</returns>
        protected string? ReadString(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn)
        {
            return Read(row, property, propertyToColumn)?.ToString().Trim();
        }

        /// <summary>
        /// Read property value as required string
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>string-value</returns>
        /// <exception cref="ArgumentException">Value is null or empty</exception>
        protected string ReadRequiredString(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn)
        {
            var value = ReadString(row, property, propertyToColumn);

            return value != null && !value.IsNullOrEmpty()
                ? value
                : throw RequiredException(property, value);
        }

        /// <summary>
        /// Read property value as nullable enum
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>Enum-value</returns>
        /// <typeparam name="TEnum">TEnum type-argument</typeparam>
        protected TEnum? ReadEnum<TEnum>(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn)
            where TEnum : struct, Enum
        {
            var value = ReadString(row, property, propertyToColumn);

            return ParseEnum<TEnum>(value);
        }

        /// <summary>
        /// Read property value as required enum
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>Enum-value</returns>
        /// <typeparam name="TEnum">TEnum type-argument</typeparam>
        /// <exception cref="ArgumentException">Value is null or empty</exception>
        protected TEnum ReadRequiredEnum<TEnum>(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn)
            where TEnum : struct, Enum
        {
            var value = ReadString(row, property, propertyToColumn);

            return ParseRequiredEnum<TEnum>(property, value);
        }

        /// <summary>
        /// Read property value as nullable decimal
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized decimal value</exception>
        /// <returns>decimal-value</returns>
        protected decimal? ReadDecimal(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn,
            IFormatProvider[] formatters)
        {
            var value = ReadString(row, property, propertyToColumn);

            return ParseDecimal(value, formatters);
        }

        /// <summary>
        /// Read property value as required decimal
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <param name="formatters">Format providers</param>
        /// <returns>decimal-value</returns>
        /// <exception cref="ArgumentException">Value is null or empty</exception>
        protected decimal ReadRequiredDecimal(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn,
            IFormatProvider[] formatters)
        {
            var value = ReadString(row, property, propertyToColumn);

            return ParseRequiredDecimal(property, value, formatters);
        }

        /// <summary>
        /// Read property value as nullable integer
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized integer value</exception>
        /// <returns>integer-value</returns>
        protected int? ReadInt(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn,
            IFormatProvider[] formatters)
        {
            var value = ReadString(row, property, propertyToColumn);

            return ParseInt(value, formatters);
        }

        /// <summary>
        /// Read property value as required integer
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <param name="formatters">Format providers</param>
        /// <returns>integer-value</returns>
        /// <exception cref="ArgumentException">Value is null or empty</exception>
        protected int ReadRequiredInt(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn,
            IFormatProvider[] formatters)
        {
            var value = ReadString(row, property, propertyToColumn);

            return ParseRequiredInt(property, value, formatters);
        }

        /// <summary>
        /// Read property value as nullable DateTime
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized DateTime value</exception>
        /// <returns>DateTime-value</returns>
        protected DateTime? ReadDateTime(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn,
            IFormatProvider[] formatters)
        {
            var value = ReadString(row, property, propertyToColumn);

            return ParseDateTime(value, formatters);
        }

        /// <summary>
        /// Read property value as required DateTime
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="property">Property name</param>
        /// <param name="propertyToColumn">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Value is null or empty</exception>
        /// <returns>DateTime-value</returns>
        protected DateTime ReadRequiredDateTime(
            DataRow row,
            string property,
            IReadOnlyDictionary<string, string> propertyToColumn,
            IFormatProvider[] formatters)
        {
            var value = ReadString(row, property, propertyToColumn);

            return ParseRequiredDateTime(property, value, formatters);
        }

        /// <summary>
        /// Parse value to nullable enum
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>Enum-value</returns>
        /// <typeparam name="TEnum">TEnum type-argument</typeparam>
        protected TEnum? ParseEnum<TEnum>(string? value)
            where TEnum : struct, Enum
        {
            if (value.IsNullOrEmpty())
            {
                return null;
            }

            return Enum.Parse<TEnum>(value, true);
        }

        /// <summary>
        /// Parse value to required enum
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">Value</param>
        /// <returns>Enum-value</returns>
        /// <typeparam name="TEnum">TEnum type-argument</typeparam>
        /// <exception cref="ArgumentException">Value is null or empty</exception>
        protected TEnum ParseRequiredEnum<TEnum>(
            string property,
            string? value)
            where TEnum : struct, Enum
        {
            return ParseEnum<TEnum>(value)
                   ?? throw RequiredException(property, value);
        }

        /// <summary>
        /// Parse value to nullable integer
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized integer value</exception>
        /// <returns>Integer-value</returns>
        protected int? ParseInt(string? value, IFormatProvider[] formatters)
        {
            return Parse(value, formatters, IntegerParser);
        }

        /// <summary>
        /// Parse value to required integer
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">value</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized integer value</exception>
        /// <returns>Integer-value</returns>
        protected int ParseRequiredInt(
            string property,
            string? value,
            IFormatProvider[] formatters)
        {
            return ParseRequired(property, value, formatters, IntegerParser);
        }

        /// <summary>
        /// Parse value to nullable decimal
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized decimal value</exception>
        /// <returns>Decimal-value</returns>
        protected decimal? ParseDecimal(
            string? value,
            IFormatProvider[] formatters)
        {
            return Parse(value, formatters, DecimalParser);
        }

        /// <summary>
        /// Parse value to required decimal
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">value</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized decimal value</exception>
        /// <returns>Decimal-value</returns>
        protected decimal ParseRequiredDecimal(
            string property,
            string? value,
            IFormatProvider[] formatters)
        {
            return ParseRequired(property, value, formatters, DecimalParser);
        }

        /// <summary>
        /// Parse value to nullable DateTime
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized DateTime value</exception>
        /// <returns>DateTime-value</returns>
        protected DateTime? ParseDateTime(
            string? value,
            IFormatProvider[] formatters)
        {
            return Parse(value, formatters, DateTimeParser);
        }

        /// <summary>
        /// Parse value to required DateTime
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">value</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized DateTime value</exception>
        /// <returns>DateTime-value</returns>
        protected DateTime ParseRequiredDateTime(
            string property,
            string? value,
            IFormatProvider[] formatters)
        {
            return ParseRequired(property, value, formatters, DateTimeParser);
        }

        private static T? Parse<T>(
            string? value,
            IFormatProvider[] formatters,
            Func<string?, IFormatProvider, T?> parser)
            where T : struct
        {
            if (value.IsNullOrEmpty())
            {
                return null;
            }

            foreach (var formatter in formatters)
            {
                var result = parser.Invoke(value, formatter);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static T ParseRequired<T>(
            string property,
            string? value,
            IFormatProvider[] formatters,
            Func<string?, IFormatProvider, T?> parser)
            where T : struct
        {
            return Parse(value, formatters, parser)
                   ?? throw RequiredException(property, value);
        }

        private static Exception RequiredException(string property, string? value)
        {
            return new ArgumentException($"Unrecognized {property} value: {value}");
        }
    }
}