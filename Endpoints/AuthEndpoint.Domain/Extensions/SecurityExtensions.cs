namespace SpaceEngineers.Core.AuthEndpoint.Domain.Extensions
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    internal static class SecurityExtensions
    {
        internal static string GenerateSalt()
        {
            var salt = new byte[64];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
                return GetBase64String(salt);
            }
        }

        internal static string GenerateSaltedHash(this string text, string salt)
        {
            var saltedText = text.GetBytes()
                .Concat(salt.GetBytes())
                .ToArray();

            using (var algorithm = new SHA256Managed())
            {
                var hashBytes = algorithm.ComputeHash(saltedText);
                return hashBytes.GetBase64String();
            }
        }

        private static byte[] GetBytes(this string source) => Encoding.UTF8.GetBytes(source);

        private static byte[] GetBase64Bytes(this string source) => Convert.FromBase64String(source);

        private static string GetBase64String(this byte[] source) => Convert.ToBase64String(source);
    }
}