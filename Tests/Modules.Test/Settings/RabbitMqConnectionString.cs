﻿namespace SpaceEngineers.Core.Modules.Test.Settings
{
    /// <summary>
    /// Class that represents connection string to RabbitMQ broker server
    /// </summary>
    public class RabbitMqConnectionString
    {
        /// <summary> .cctor </summary>
        /// <param name="host">Host</param>
        /// <param name="virtualHost">VirtualHost</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public RabbitMqConnectionString(string host, string virtualHost, string username, string password)
        {
            Host = host;
            VirtualHost = virtualHost;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// VirtualHost
        /// </summary>
        public string VirtualHost { get; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; }

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