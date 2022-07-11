namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    using System;

    /// <summary>
    /// SchemaAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
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