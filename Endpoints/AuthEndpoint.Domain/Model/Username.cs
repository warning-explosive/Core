namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using System.Diagnostics;
    using Basics;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.Api.Exceptions;

    /// <summary>
    /// Username value object
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public class Username : IValueObject
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Username value</param>
        public Username(string value)
        {
            if (value.IsNullOrEmpty())
            {
                throw new DomainInvariantViolationException("Username should have not nullable value");
            }

            Value = value.Length switch
            {
                < 5 => throw new DomainInvariantViolationException("Username length should be greater or equal than 5 symbols"),
                > 32 => throw new DomainInvariantViolationException("Username length should be less or equal than 32 symbols"),
                _ => value
            };

            Value = value;
        }

        /// <summary>
        /// Username value
        /// </summary>
        public string Value { get; init; }

        /// <inheritdoc />
        public sealed override string ToString()
        {
            return Value;
        }
    }
}