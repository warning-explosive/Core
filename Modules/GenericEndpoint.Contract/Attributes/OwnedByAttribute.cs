namespace SpaceEngineers.Core.GenericEndpoint.Contract.Attributes
{
    using System;

    /// <summary>
    /// Specifies logical (not physical) owner for events and commands
    /// Event owner - publisher endpoint
    /// Command owner - send operation target endpoint
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OwnedByAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="endpointName">Logical endpoint name</param>
        public OwnedByAttribute(string endpointName)
        {
            EndpointName = endpointName;
        }

        /// <summary>
        /// Logical endpoint name
        /// </summary>
        public string EndpointName { get; }
    }
}
