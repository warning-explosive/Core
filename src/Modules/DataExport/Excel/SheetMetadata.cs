namespace SpaceEngineers.Core.DataExport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Spreadsheet;

    internal class SheetMetadata
    {
        private const string DateTimeFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";

        public SheetMetadata(DocumentInfo documentInfo)
            : this(documentInfo, new ColumnNameProducer())
        {
        }

        private SheetMetadata(
            DocumentInfo documentInfo,
            ColumnNameProducer columnNameProducer)
        {
            DocumentInfo = documentInfo;
            ColumnNameProducer = columnNameProducer;
        }

        public DocumentInfo DocumentInfo { get; }

        public ColumnNameProducer ColumnNameProducer { get; }

        public (PropertyInfo[], IReadOnlyDictionary<PropertyInfo, CellValues>) GetProperties(
            Type type,
            IReadOnlyCollection<string> columnsOrder)
        {
            var columnsPriorities = columnsOrder
                .Select((value, index) => (value, index))
                .ToDictionary(
                    it => it.value,
                    it => it.index,
                    StringComparer.OrdinalIgnoreCase);

            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
                .Where(property => !property.PropertyType.IsCollection())
                .OrderBy(property => columnsPriorities.TryGetValue(property.Name, out var priority)
                    ? priority
                    : int.MaxValue)
                .ToArray();

            var cellValuesMap = properties
                .ToDictionary(
                    it => it,
                    it => GetDataType(it.PropertyType));

            return (properties, cellValuesMap);

            static CellValues GetDataType(Type type)
            {
                type = type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

                if (type.IsNumeric())
                {
                    return CellValues.Number;
                }

                if (type == typeof(DateTime))
                {
                    return CellValues.Date;
                }

                return CellValues.SharedString;
            }
        }

        public Cell GetCellValue(
            object? rawValue,
            CellValues dataType,
            uint rowIndex,
            int columnIndex)
        {
            var cellReference = ColumnNameProducer.GetCellReference(columnIndex, rowIndex);
            var value = ConvertValue(rawValue);

            if (dataType == CellValues.SharedString)
            {
                if (!DocumentInfo.SharedStringIndexCounter.TryGetIndex(value, out var index))
                {
                    _ = DocumentInfo
                        .SharedStringTable
                        .AppendChild(new SharedStringItem(new Text(value)));

                    index = DocumentInfo.SharedStringIndexCounter.Next(value);
                }

                return new Cell
                {
                    CellReference = cellReference,
                    DataType = new EnumValue<CellValues>(dataType),
                    CellValue = new CellValue(index),
                    StyleIndex = DocumentInfo.DefaultCellFormatIndex
                };
            }

            if (dataType == CellValues.InlineString)
            {
                return new Cell
                {
                    CellReference = cellReference,
                    DataType = new EnumValue<CellValues>(dataType),
                    InlineString = new InlineString(new Text(value)),
                    StyleIndex = DocumentInfo.DefaultCellFormatIndex
                };
            }

            if (dataType == CellValues.Number)
            {
                return new Cell
                {
                    CellReference = cellReference,
                    DataType = new EnumValue<CellValues>(dataType),
                    CellValue = new CellValue(value),
                    StyleIndex = DocumentInfo.DecimalCellFormatIndex
                };
            }

            if (dataType == CellValues.Date)
            {
                return new Cell
                {
                    CellReference = cellReference,
                    DataType = new EnumValue<CellValues>(dataType),
                    CellValue = new CellValue(value),
                    StyleIndex = DocumentInfo.DateCellFormatIndex
                };
            }

            return new Cell
            {
                CellReference = cellReference,
                DataType = new EnumValue<CellValues>(dataType),
                CellValue = new CellValue(value),
                StyleIndex = DocumentInfo.DefaultCellFormatIndex
            };

            static string ConvertValue(object? value)
            {
                if (value == null)
                {
                    return string.Empty;
                }

                var type = value.GetType().ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

                if (type.IsNumeric())
                {
                    return Convert
                        .ToDouble(value, CultureInfo.InvariantCulture)
                        .ToString(CultureInfo.InvariantCulture);
                }

                if (type == typeof(DateTime))
                {
                    return ((DateTime)value).ToString(DateTimeFormatString, CultureInfo.InvariantCulture);
                }

                if (type == typeof(string))
                {
                    return (string)value;
                }

                return value.ToString() ?? string.Empty;
            }
        }
    }
}