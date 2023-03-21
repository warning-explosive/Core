namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System.Collections.Generic;
    using Basics;

    /// <summary>
    /// CreateEnumType
    /// </summary>
    public class CreateEnumType : IModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="type">Type</param>
        /// <param name="values">Values</param>
        public CreateEnumType(
            string schema,
            string type,
            IReadOnlyCollection<string> values)
        {
            Schema = schema;
            Type = type;
            Values = values;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Values
        /// </summary>
        public IReadOnlyCollection<string> Values { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(CreateEnumType)} {Schema}.{Type} ({Values.ToString(", ")})";
        }
    }
}