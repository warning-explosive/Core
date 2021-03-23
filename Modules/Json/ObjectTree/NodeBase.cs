namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree
{
    /// <summary>
    /// Base ObjectTree node
    /// </summary>
    public abstract class NodeBase : IObjectTreeNode
    {
        /// <summary> .cctor </summary>
        /// <param name="parent">Node parent</param>
        /// <param name="path">Node path</param>
        protected NodeBase(IObjectTreeNode? parent, string path)
        {
            Parent = parent;
            Depth = (parent?.Depth ?? 0) + 1;
            Path = path;
        }

        /// <inheritdoc />
        public IObjectTreeNode? Parent { get; }

        /// <inheritdoc />
        public int Depth { get; }

        /// <inheritdoc />
        public string Path { get; }

        /// <inheritdoc />
        public abstract void Add(IObjectTreeNode child);

        /// <inheritdoc />
        public abstract object? ExtractTree();

        /// <inheritdoc />
        public abstract IObjectTreeNode? ExtractPath(params string[] path);
    }
}