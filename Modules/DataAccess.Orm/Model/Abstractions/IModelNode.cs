namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using System;

    /// <summary>
    /// IModelNode
    /// </summary>
    public interface IModelNode
    {
        /// <summary>
        /// Type
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }
    }
}