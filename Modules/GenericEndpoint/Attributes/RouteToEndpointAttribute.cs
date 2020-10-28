namespace SpaceEngineers.Core.GenericEndpoint.Attributes
{
    using System;

    /// <summary>
    /// Route command to specified endpoint
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RouteToEndpointAttribute : Attribute
    {
        /// <summary> .cctor </summary>
        /// <param name="endpointName">Logical endpoint name</param>
        public RouteToEndpointAttribute(string endpointName)
        {
            EndpointName = endpointName;
        }

        /// <summary>
        /// Logical endpoint name
        /// </summary>
        public string EndpointName { get; }
    }
}