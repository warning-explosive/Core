namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// CreateIndex
    /// </summary>
    public class CreateIndex : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="index">Name</param>
        /// <param name="columns">Columns</param>
        /// <param name="unique">Unique</param>
        public CreateIndex(
            string schema,
            string table,
            string index,
            IReadOnlyCollection<string> columns,
            bool unique)
        {
            Schema = schema;
            Table = table;
            Index = index;
            Columns = columns;
            Unique = unique;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Index
        /// </summary>
        public string Index { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<string> Columns { get; }

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateIndex)} {Schema}.{Table}.{Index}";
        }
    }
}