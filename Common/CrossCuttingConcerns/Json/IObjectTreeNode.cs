namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    /// <summary>
    /// IObjectTreeNodeNode
    /// </summary>
    public interface IObjectTreeNode
    {
        /// <summary>
        /// Node parent
        /// </summary>
        public IObjectTreeNode? Parent { get; }

        /// <summary>
        /// Node composition depth
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Node path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Add child to node
        /// </summary>
        /// <param name="child">Child IObjectTreeNodeNode</param>
        void Add(IObjectTreeNode child);

        /// <summary>
        /// Build object-tree from node
        /// </summary>
        /// <returns>Object-tree</returns>
        object? ExtractTree();

        /// <summary>
        /// Extract node by specified path
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Object-tree</returns>
        IObjectTreeNode? ExtractPath(params string[] path);
    }
}