namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes
{
    using System;
    using System.Linq;

    /// <summary>
    /// IndexAttribute
    /// </summary>
    // TODO: #202 - add sql dependent features like clustered indexes, filtered indexes, included fields and so on
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
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
                .ToArray();

            IncludedColumns = Array.Empty<string>();
        }

        /// <summary>
        /// Columns
        /// </summary>
        public string[] Columns { get; }

        /// <summary>
        /// Included columns
        /// </summary>
        public string[] IncludedColumns { get; set; }

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; set; }
    }
}