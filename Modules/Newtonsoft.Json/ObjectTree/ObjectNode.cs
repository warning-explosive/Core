namespace SpaceEngineers.Core.NewtonSoft.Json.ObjectTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ObjectNode - associative array of nodes
    /// </summary>
    public class ObjectNode : IObjectTreeNode
    {
        /// <summary> .cctor </summary>
        /// <param name="parent">Parent node</param>
        public ObjectNode(IObjectTreeNode? parent)
        {
            Parent = parent;
            Members = new Dictionary<string, IObjectTreeNode>();
            Depth = (parent?.Depth ?? 0) + 1;
        }

        /// <inheritdoc />
        public int Depth { get; }

        /// <inheritdoc />
        public IObjectTreeNode? Parent { get; }

        /// <summary>
        /// Members
        /// </summary>
        public IDictionary<string, IObjectTreeNode> Members { get; }

        /// <summary>
        /// CurrentProperty
        /// </summary>
        public string? CurrentProperty { get; set; }

        /// <inheritdoc />
        public void Add(IObjectTreeNode child)
        {
            if (CurrentProperty == null
             || !Members.ContainsKey(CurrentProperty))
            {
                throw new InvalidOperationException("Property is null or isn't presented in object");
            }

            Members[CurrentProperty] = child;
        }

        /// <inheritdoc />
        public object? ExtractTree()
        {
            return Members.ToDictionary(m => m.Key, m => m.Value.ExtractTree());
        }
    }
}