namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using Basics;
    using Extensions;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.Api.Exceptions;

    /// <summary>
    /// Password
    /// </summary>
    public class Password : IValueObject
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Username value</param>
        public Password(string value)
        {
            if (value.IsNullOrEmpty())
            {
                throw new DomainInvariantViolationException("Password should have not nullable value");
            }

            Value = value.Length switch
            {
                < 8 => throw new DomainInvariantViolationException("Password length should be greater or equal than 8 symbols"),
                > 256 => throw new DomainInvariantViolationException("Password length should be less or equal than 256 symbols"),
                _ => value
            };
        }

        /// <summary>
        /// Username value
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// Generates password hash
        /// </summary>
        /// <param name="salt">Salt</param>
        /// <returns>Password hash</returns>
        public string GeneratePasswordHash(string salt)
        {
            return Value.GenerateSaltedHash(salt);
        }

        /// <inheritdoc />
        public sealed override string ToString()
        {
            return Value;
        }
    }
}