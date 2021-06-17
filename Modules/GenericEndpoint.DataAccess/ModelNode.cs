namespace SpaceEngineers.Core.GenericEndpoint.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ModelNode
    /// </summary>
    public class ModelNode
    {
        /// <summary> .cctor </summary>
        /// <param name="nodeType">Node type</param>
        /// <param name="childNodes">Child nodes</param>
        public ModelNode(Type nodeType, IEnumerable<ModelNode> childNodes)
        {
            NodeType = nodeType;
            ChildNodes = childNodes.ToList();
        }

        /// <summary>
        /// Node type
        /// </summary>
        public Type NodeType { get; }

        /// <summary>
        /// Child nodes
        /// </summary>
        public IReadOnlyCollection<ModelNode> ChildNodes { get; }
    }
}