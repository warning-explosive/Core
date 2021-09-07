namespace SpaceEngineers.Core.IntegrationTransport.Settings
{
    using System;
    using CrossCuttingConcerns.Api.Abstractions;

    /// <summary>
    /// IntegrationTransportSettings
    /// </summary>
    public class IntegrationTransportSettings : IYamlSettings
    {
        /// <summary> .cctor </summary>
        public IntegrationTransportSettings()
        {
            RpcRequestSecondsTimeout = 60;
        }

        /// <summary>
        /// Rpc request timeout (seconds)
        /// </summary>
        public uint RpcRequestSecondsTimeout { get; set; }

        /// <summary>
        /// Rpc request timeout
        /// </summary>
        public TimeSpan RpcRequestTimeout => TimeSpan.FromSeconds(RpcRequestSecondsTimeout);
    }
}