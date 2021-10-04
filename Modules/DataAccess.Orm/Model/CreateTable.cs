namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// CreateTable
    /// </summary>
    public class CreateTable : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="type">Type</param>
        /// <param name="columns">Columns</param>
        public CreateTable(string schema,
            string table,
            Type type,
            IReadOnlyCollection<CreateColumn> columns)
        {
            Schema = schema;
            Table = table;
            Type = type;
            Columns = columns;
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
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<CreateColumn> Columns { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateTable)} {Schema}.{Table} ({Type.FullName})";
        }
    }
}