namespace SpaceEngineers.Core.Json.ObjectTree
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ArrayNode - collection of nodes
    /// </summary>
    public class ArrayNode : NodeBase
    {
        /// <summary> .cctor </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="path">Node path</param>
        public ArrayNode(IObjectTreeNode? parent, string path)
            : base(parent, path)
        {
            Items = new List<IObjectTreeNode>();
        }

        /// <summary>
        /// Items
        /// </summary>
        public ICollection<IObjectTreeNode> Items { get; }

        /// <inheritdoc />
        public override void Add(IObjectTreeNode child)
        {
            Items.Add(child);
        }

        /// <inheritdoc />
        public override object? ExtractTree()
        {
            return Items.Select(i => i.ExtractTree()).ToList();
        }

        /// <inheritdoc />
        public override IObjectTreeNode? ExtractPath(params string[] path)
        {
            if (!path.Any())
            {
                return this;
            }

            var next = path.First();

            if (int.TryParse(next, out var index)
             && index >= 0
             && index < Items.Count)
            {
                return Items.ElementAt(index).ExtractPath(path.Skip(1).ToArray());
            }

            return null;
        }
    }
}