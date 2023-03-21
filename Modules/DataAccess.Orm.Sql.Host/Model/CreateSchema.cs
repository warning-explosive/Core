﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    /// <summary>
    /// CreateSchema
    /// </summary>
    public class CreateSchema : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        public CreateSchema(string schema)
        {
            Schema = schema;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateSchema)} {Schema}";
        }
    }
}