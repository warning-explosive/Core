namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ObjectNode - associative array of nodes
    /// </summary>
    public class ObjectNode : NodeBase
    {
        /// <summary> .cctor </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="path">Node path</param>
        public ObjectNode(IObjectTreeNode? parent, string path)
            : base(parent, path)
        {
            Members = new Dictionary<string, IObjectTreeNode>();
        }

        /// <summary>
        /// Members
        /// </summary>
        public IDictionary<string, IObjectTreeNode> Members { get; }

        /// <summary>
        /// CurrentProperty
        /// </summary>
        public string? CurrentProperty { get; set; }

        /// <inheritdoc />
        public override void Add(IObjectTreeNode child)
        {
            if (CurrentProperty == null
             || !Members.ContainsKey(CurrentProperty))
            {
                throw new InvalidOperationException("Property is null or isn't presented in object");
            }

            Members[CurrentProperty] = child;
        }

        /// <inheritdoc />
        public override object? ExtractTree()
        {
            return Members.ToDictionary(m => m.Key, m => m.Value.ExtractTree());
        }

        /// <inheritdoc />
        public override IObjectTreeNode? ExtractPath(params string[] path)
        {
            if (!path.Any())
            {
                return this;
            }

            var next = path.First();

            if (Members.ContainsKey(next))
            {
                return Members[next].ExtractPath(path.Skip(1).ToArray());
            }

            return null;
        }
    }
}