namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;

    /// <summary>
    /// SqlCommandParameter
    /// </summary>
    public class SqlCommandParameter
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        // TODO: check creations
        public SqlCommandParameter(string name, object? value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Value
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Deconstruct
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        public void Deconstruct(
            out string name,
            out object? value,
            out Type type)
        {
            name = Name;
            value = Value;
            type = Type;
        }
    }
}