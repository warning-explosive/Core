namespace SpaceEngineers.Core.Json.ObjectTree
{
    using System;
    using System.Linq;

    /// <summary>
    /// RootNode - object-tree root
    /// </summary>
    public class RootNode : NodeBase
    {
        /// <summary> .cctor </summary>
        public RootNode()
            : base(null, ".")
        {
        }

        /// <summary>
        /// Child node
        /// </summary>
        public IObjectTreeNode? Child { get; private set; }

        /// <inheritdoc />
        public override void Add(IObjectTreeNode child)
        {
            if (Child == null)
            {
                Child = child;
                return;
            }

            throw new InvalidOperationException("Trying to add child twice into the RootNode");
        }

        /// <inheritdoc />
        public override object? ExtractTree()
        {
            return Child;
        }

        /// <inheritdoc />
        public override IObjectTreeNode? ExtractPath(params string[] path)
        {
            if (!path.Any())
            {
                return this;
            }

            var next = path.First();

            return next == "."
                       ? Child?.ExtractPath(path.Skip(1).ToArray())
                       : throw new InvalidOperationException($"Invalid root path: '{next}'. Use '.' instead");
        }
    }
}