namespace SpaceEngineers.Core.DataExport.Excel
{
    using System.Collections.Generic;
    using Basics;

    internal class ColumnNameProducer
    {
        private readonly IDictionary<int, string> _map = new Dictionary<int, string>();

        public string GetCellReference(int columnIndex, uint rowIndex)
        {
            return $"{GetColumnName(columnIndex)}{rowIndex}";
        }

        private string GetColumnName(int index)
        {
            if (_map.TryGetValue(index, out var value))
            {
                return value;
            }

            value = index.AlphabetIndex();
            _map[index] = value;

            return value;
        }
    }
}