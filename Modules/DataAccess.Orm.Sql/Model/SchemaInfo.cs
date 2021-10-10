namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// SchemaInfo
    /// </summary>
    public class SchemaInfo : IModelInfo
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="tables">Tables</param>
        /// <param name="views">Views</param>
        public SchemaInfo(
            string schema,
            IReadOnlyCollection<TableInfo> tables,
            IReadOnlyCollection<ViewInfo> views)
        {
            Schema = schema;
            Tables = tables;
            Views = views;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Tables
        /// </summary>
        public IReadOnlyCollection<TableInfo> Tables { get; }

        /// <summary>
        /// Views
        /// </summary>
        public IReadOnlyCollection<ViewInfo> Views { get; }
    }
}