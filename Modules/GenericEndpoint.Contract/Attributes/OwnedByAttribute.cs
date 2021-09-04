namespace SpaceEngineers.Core.GenericEndpoint.Contract.Attributes
{
    using System;

    /// <summary>
    /// Specifies logical (not physical) owner for contract messages
    /// Command owner - endpoint which can handle that command
    /// Event owner - endpoint which publishes event
    /// Query owner - endpoint which can handle that query
    /// Reply has no owner
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
