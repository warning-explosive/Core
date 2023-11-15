namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using Basics;

    /// <summary>
    /// TriggerNode
    /// </summary>
    public class TriggerNode : IEquatable<TriggerNode>,
                               ISafelyEquatable<TriggerNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="trigger">Trigger</param>
        /// <param name="table">Table</param>
        /// <param name="function">Function</param>
        /// <param name="type">Type</param>
        /// <param name="event">Event</param>
        public TriggerNode(
            string schema,
            string trigger,
            string table,
            string function,
            EnTriggerType type,
            EnTriggerEvent @event)
        {
            Schema = schema;
            Trigger = trigger;
            Table = table;
            Function = function;
            Type = type;
            Event = @event;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Trigger
        /// </summary>
        public string Trigger { get; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Function
        /// </summary>
        public string Function { get; }

        /// <summary>
        /// Type
        /// </summary>
        public EnTriggerType Type { get; }

        /// <summary>
        /// Event
        /// </summary>
        public EnTriggerEvent Event { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left TriggerNode</param>
        /// <param name="right">Right TriggerNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(TriggerNode? left, TriggerNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left TriggerNode</param>
        /// <param name="right">Right TriggerNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(TriggerNode? left, TriggerNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Trigger.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Table.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Function.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Type,
                Event);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(TriggerNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(TriggerNode other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Trigger.Equals(other.Trigger, StringComparison.OrdinalIgnoreCase)
                   && Table.Equals(other.Table, StringComparison.OrdinalIgnoreCase)
                   && Function.Equals(other.Function, StringComparison.OrdinalIgnoreCase)
                   && Type == other.Type
                   && Event == other.Event;
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Trigger} ({Table}, {Function}, {Type}, {Event})";
        }
    }
}