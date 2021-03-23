namespace SpaceEngineers.Core.Json.ObjectTree
{
    using System;
    using System.Linq;

    /// <summary>
    /// ValueNode - simple leaf-node
    /// </summary>
    public sealed class ValueNode : NodeBase
    {
        /// <summary> .cctor </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="value">NodeValue</param>
        /// <param name="path">Node path</param>
        public ValueNode(IObjectTreeNode? parent, string path, object? value = null)
            : base(parent, path)
        {
            Value = value;
        }

        /// <summary>
        /// Node value
        /// </summary>
        public object? Value { get; }

        /// <inheritdoc />
        public override void Add(IObjectTreeNode child)
        {
            throw new InvalidOperationException("Trying to add a child into the leaf-node");
        }

        /// <inheritdoc />
        public override object? ExtractTree()
        {
            return Value;
        }

        /// <inheritdoc />
        public override IObjectTreeNode? ExtractPath(params string[] path)
        {
            return path.Any() ? null : this;
        }
    }
}