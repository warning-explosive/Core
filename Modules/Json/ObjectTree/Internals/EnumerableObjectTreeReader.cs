namespace SpaceEngineers.Core.Json.ObjectTree.Internals
{
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Enumerable abstraction for object-tree deserialization
    /// </summary>
    internal sealed class EnumerableObjectTreeReader : IEnumerable<IObjectTreeNode>
    {
        private readonly JsonReader _jsonReader;

        private readonly IObjectTreeValueReader _objectTreeValueReader;

        /// <summary> .cctor </summary>
        /// <param name="jsonReader">JsonReader</param>
        /// <param name="objectTreeValueReader">IObjectTreeValueReader</param>
        public EnumerableObjectTreeReader(JsonReader jsonReader, IObjectTreeValueReader objectTreeValueReader)
        {
            _jsonReader = jsonReader;
            _objectTreeValueReader = objectTreeValueReader;
        }

        /// <inheritdoc />
        public IEnumerator<IObjectTreeNode> GetEnumerator()
        {
            return new ObjectTreeJsonEnumerator(_jsonReader, _objectTreeValueReader);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}