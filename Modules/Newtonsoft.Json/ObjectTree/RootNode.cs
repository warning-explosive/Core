namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree
{
    using System;

    /// <summary>
    /// RootNode - object-tree root
    /// </summary>
    public class RootNode : IObjectTreeNode
    {
        /// <summary> .cctor </summary>
        public RootNode()
        {
            Parent = null;
            Depth = 0;
        }

        /// <inheritdoc />
        public int Depth { get; }

        /// <inheritdoc />
        public IObjectTreeNode? Parent { get; }

        /// <summary>
        /// Child node
        /// </summary>
        public IObjectTreeNode? Child { get; private set; }

        /// <inheritdoc />
        public void Add(IObjectTreeNode child)
        {
            if (Child == null)
            {
                Child = child;
                return;
            }

            throw new InvalidOperationException("Trying to add child twice into the RootNode");
        }

        /// <inheritdoc />
        public object? ExtractTree()
        {
            return Child;
        }
    }
}