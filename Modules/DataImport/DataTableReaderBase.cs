namespace SpaceEngineers.Core.DataImport
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using Abstractions;
    using Basics;

    /// <inheritdoc />
    public abstract class DataTableReaderBase<TElement> : IDataTableReader<TElement>
    {
        /// <inheritdoc />
        public abstract IReadOnlyDictionary<string, string> PropertyToColumnCaption { get; }

        /// <inheritdoc />
        public abstract TElement? ReadRow(DataRow row, IReadOnlyDictionary<string, string> propertyToColumn);

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
            return Read(row, property, propertyToColumn)?.ToString();
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
                : throw RequiredException(property);
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
            return ReadEnum<TEnum>(row, property, propertyToColumn)
                   ?? throw RequiredException(property);
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

            if (value.IsNullOrEmpty())
            {
                return null;
            }

            foreach (var formatter in formatters)
            {
                if (decimal.TryParse(value, NumberStyles.Any, formatter, out var amount))
                {
                    return amount;
                }
            }

            throw new ArgumentException($"Unrecognized decimal: {value}");
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
            return ReadDecimal(row, property, propertyToColumn, formatters)
                   ?? throw RequiredException(property);
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

            if (value.IsNullOrEmpty())
            {
                return null;
            }

            foreach (var formatter in formatters)
            {
                if (int.TryParse(value, NumberStyles.Any, formatter, out var amount))
                {
                    return amount;
                }
            }

            throw new ArgumentException($"Unrecognized integer: {value}");
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
            return ReadInt(row, property, propertyToColumn, formatters)
                   ?? throw RequiredException(property);
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
            return ReadDateTime(row, property, propertyToColumn, formatters)
                   ?? throw RequiredException(property);
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
        /// <param name="value">Value</param>
        /// <param name="property">Property name</param>
        /// <returns>Enum-value</returns>
        /// <typeparam name="TEnum">TEnum type-argument</typeparam>
        /// <exception cref="ArgumentException">Value is null or empty</exception>
        protected TEnum ParseRequiredEnum<TEnum>(
            string? value,
            string property)
            where TEnum : struct, Enum
        {
            return ParseEnum<TEnum>(value)
                   ?? throw RequiredException(property);
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
            if (value.IsNullOrEmpty())
            {
                return null;
            }

            foreach (var formatter in formatters)
            {
                if (DateTime.TryParse(value, formatter, DateTimeStyles.AllowWhiteSpaces, out var dateTime))
                {
                    return dateTime;
                }
            }

            throw new ArgumentException($"Unrecognized DateTime: {value}");
        }

        /// <summary>
        /// Parse value to required DateTime
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="property">Property name</param>
        /// <param name="formatters">Format providers</param>
        /// <exception cref="ArgumentException">Unrecognized DateTime value</exception>
        /// <returns>DateTime-value</returns>
        protected DateTime ParseRequiredDateTime(
            string? value,
            string property,
            IFormatProvider[] formatters)
        {
            return ParseDateTime(value, formatters)
                   ?? throw RequiredException(property);
        }

        private static Exception RequiredException(string property)
        {
            return new ArgumentException($"{property} is required");
        }
    }
}