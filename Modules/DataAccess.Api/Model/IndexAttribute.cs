namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IndexAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class IndexAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="column">Column</param>
        /// <param name="columns">Columns</param>
        public IndexAttribute(string column, params string[] columns)
        {
            Columns = new[] { column }
                .Concat(columns)
                .OrderBy(col => col)
                .ToList();
        }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<string> Columns { get; }

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; set; }
    }
}