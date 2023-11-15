namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes
{
    using System;

    /// <summary>
    /// ForeignKeyAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ForeignKeyAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="onDeleteBehavior">EnDeleteBehavior</param>
        public ForeignKeyAttribute(EnOnDeleteBehavior onDeleteBehavior)
        {
            OnDeleteBehavior = onDeleteBehavior;
        }

        /// <summary>
        /// Delete behavior
        /// </summary>
        public EnOnDeleteBehavior OnDeleteBehavior { get; }
    }
}