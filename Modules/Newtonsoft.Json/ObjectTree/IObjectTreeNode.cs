namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree
{
    /// <summary>
    /// IObjectTreeNodeNode
    /// </summary>
    public interface IObjectTreeNode
    {
        /// <summary>
        /// Node composition depth
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Node parent
        /// </summary>
        public IObjectTreeNode? Parent { get; }

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
    }
}