namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ArrayNode - collection of nodes
    /// </summary>
    public class ArrayNode : IObjectTreeNode
    {
        /// <summary> .cctor </summary>
        /// <param name="parent">Parent node</param>
        public ArrayNode(IObjectTreeNode? parent)
        {
            Parent = parent;
            Items = new List<IObjectTreeNode>();
            Depth = (parent?.Depth ?? 0) + 1;
        }

        /// <inheritdoc />
        public int Depth { get; }

        /// <inheritdoc />
        public IObjectTreeNode? Parent { get; }

        /// <summary>
        /// Items
        /// </summary>
        public ICollection<IObjectTreeNode> Items { get; }

        /// <inheritdoc />
        public void Add(IObjectTreeNode child)
        {
            Items.Add(child);
        }

        /// <inheritdoc />
        public object? ExtractTree()
        {
            return Items.Select(i => i.ExtractTree()).ToList();
        }
    }
}