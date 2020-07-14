﻿namespace SpaceEngineers.Core.GenericEndpoint.Conventions
{
    /// <summary>
    /// Class that represents connection string to RabbitMQ broker server
    /// </summary>
    public class RabbitMqConnectionString
    {
        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// VirtualHost
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Use Transport Layer Security
        /// </summary>
        public bool UseTls { get; set; } = false;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Host)}={Host};"
                 + $"{nameof(VirtualHost)}={VirtualHost};"
                 + $"{nameof(Username)}={Username};"
                 + $"{nameof(Password)}={Password};"
                 + $"{nameof(UseTls)}={UseTls};";
        }
    }
}