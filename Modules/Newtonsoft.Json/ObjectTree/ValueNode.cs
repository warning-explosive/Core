namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree
{
    using System;

    /// <summary>
    /// ValueNode - simple leaf-node
    /// </summary>
    public sealed class ValueNode : IObjectTreeNode
    {
        /// <summary> .cctor </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="value">NodeValue</param>
        public ValueNode(IObjectTreeNode? parent, object? value = null)
        {
            Parent = parent;
            Value = value;
            Depth = (parent?.Depth ?? 0) + 1;
        }

        /// <inheritdoc />
        public int Depth { get; }

        /// <inheritdoc />
        public IObjectTreeNode? Parent { get; }

        /// <summary>
        /// Node value
        /// </summary>
        public object? Value { get; }

        /// <inheritdoc />
        public void Add(IObjectTreeNode child)
        {
            throw new InvalidOperationException("Trying to add a child into the leaf-node");
        }

        /// <inheritdoc />
        public object? ExtractTree()
        {
            return Value;
        }
    }
}