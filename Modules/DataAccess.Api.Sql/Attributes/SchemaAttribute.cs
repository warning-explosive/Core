namespace SpaceEngineers.Core.DataAccess.Api.Sql.Attributes
{
    using System;

    /// <summary>
    /// SchemaAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SchemaAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        public SchemaAttribute(string schema)
        {
            Schema = schema;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }
    }
}