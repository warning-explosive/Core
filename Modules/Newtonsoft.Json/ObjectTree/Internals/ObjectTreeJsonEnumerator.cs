namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree.Internals
{
    using System.Collections;
    using System.Collections.Generic;
    using Basics;
    using Newtonsoft.Json;

    internal class ObjectTreeJsonEnumerator : IEnumerator<IObjectTreeNode>
    {
        private readonly JsonReader _reader;

        private readonly IObjectTreeValueReader _objectTreeValueReader;

        private IObjectTreeNode _current;

        /// <summary> .cctor </summary>
        /// <param name="reader">JsonReader</param>
        /// <param name="objectTreeValueReader">IObjectTreeValueReader</param>
        public ObjectTreeJsonEnumerator(JsonReader reader, IObjectTreeValueReader objectTreeValueReader)
        {
            _current = new RootNode();
            _reader = reader;
            _objectTreeValueReader = objectTreeValueReader;
        }

        /// <inheritdoc />
        public IObjectTreeNode Current => _current;

        /// <inheritdoc />
        object? IEnumerator.Current => _current;

        private IObjectTreeNode CurrentParent => _current.Parent.EnsureNotNull("Parent cannot be null");

        /// <inheritdoc />
        public bool MoveNext()
        {
            bool read;
            IObjectTreeNode next;

            switch (_reader.TokenType)
            {
                case JsonToken.StartObject:
                    IObjectTreeNode child = new ObjectNode(_current, _reader.Path);
                    _current.Add(child);
                    next = child;
                    read = _reader.Read();
                    break;
                case JsonToken.EndObject when _current is ObjectNode objectNode:
                    next = CurrentParent;
                    objectNode.CurrentProperty = null;
                    read = _reader.Read();
                    break;
                case JsonToken.StartArray:
                    child = new ArrayNode(_current, _reader.Path);
                    _current.Add(child);
                    next = child;
                    read = _reader.Read();
                    break;
                case JsonToken.EndArray:
                    next = CurrentParent;
                    read = _reader.Read();
                    break;
                case JsonToken.PropertyName when _current is ObjectNode objectNode:
                    var property = _reader.Value.EnsureNotNull("null named property").ToString();
                    objectNode.Members.Add(property, new ValueNode(objectNode, _reader.Path));
                    objectNode.CurrentProperty = property;
                    next = _current;
                    read = _reader.Read();
                    break;
                default:
                    ValueNode value;
                    (read, value) = _objectTreeValueReader.Read(_reader, _current);
                    _current.Add(value);
                    next = _current;
                    break;
            }

            if (read)
            {
                _current = next;
            }

            return read;
        }

        /// <inheritdoc />
        public void Reset()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}