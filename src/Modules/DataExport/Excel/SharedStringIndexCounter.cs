namespace SpaceEngineers.Core.DataExport.Excel
{
    using System;
    using System.Collections.Generic;

    internal class SharedStringIndexCounter
    {
        private readonly IDictionary<string, int> _map = new Dictionary<string, int>(StringComparer.Ordinal);

        private int _counter;

        public int Next(string stringValue)
        {
            var index = _counter;
            _map.Add(stringValue, index);
            _counter++;
            return index;
        }

        public bool TryGetIndex(string stringValue, out int index)
        {
            return _map.TryGetValue(stringValue, out index);
        }
    }
}