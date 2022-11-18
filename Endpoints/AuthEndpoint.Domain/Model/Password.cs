namespace SpaceEngineers.Core.AuthEndpoint.Domain.Model
{
    using System;
    using System.Security.Cryptography;
    using Basics;
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
                > 32 => throw new DomainInvariantViolationException("Password length should be less or equal than 32 symbols"),
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
            using (var pbkdf2 = new Rfc2898DeriveBytes(Value, Convert.FromBase64String(salt), 100_000, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(32));
            }
        }

        /// <inheritdoc />
        public sealed override string ToString()
        {
            return Value;
        }

        internal static string GenerateSalt()
        {
            var salt = new byte[32];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);

                return Convert.ToBase64String(salt);
            }
        }
    }
}