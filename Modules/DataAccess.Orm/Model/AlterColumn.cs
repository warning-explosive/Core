namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;

    /// <summary>
    /// AlterColumn
    /// </summary>
    public class AlterColumn : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="column">Column</param>
        /// <param name="type">Type</param>
        public AlterColumn(
            string schema,
            string table,
            string column,
            Type type)
        {
            Schema = schema;
            Table = table;
            Column = column;
            Type = type;
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
        /// Column
        /// </summary>
        public string Column { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(AlterColumn)} {Schema}.{Table}.{Column} ({Type.FullName})";
        }
    }
}