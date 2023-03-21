namespace SpaceEngineers.Core.GenericEndpoint.Settings
{
    using System;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    /// <summary>
    /// GenericEndpointSettings
    /// </summary>
    public class GenericEndpointSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public GenericEndpointSettings()
        {
            RpcRequestSecondsTimeout = 60;
        }

        /// <summary>
        /// Rpc request timeout (seconds)
        /// </summary>
        public uint RpcRequestSecondsTimeout { get; init; }

        /// <summary>
        /// Rpc request timeout
        /// </summary>
        public TimeSpan RpcRequestTimeout => TimeSpan.FromSeconds(RpcRequestSecondsTimeout);
    }
}