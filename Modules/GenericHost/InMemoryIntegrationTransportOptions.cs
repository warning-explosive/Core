namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Collections.Generic;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using Defaults;

    /// <summary>
    /// In-memory integration transport options
    /// </summary>
    public class InMemoryIntegrationTransportOptions
    {
        /// <summary>
        /// Endpoint instance selection behavior
        /// </summary>
        /// <remarks>Default: DefaultEndpointInstanceSelectionBehavior</remarks>
        public IEndpointInstanceSelectionBehavior EndpointInstanceSelectionBehavior { get; set; }
            = new DefaultEndpointInstanceSelectionBehavior();

        /// <summary>
        /// Additional manual registrations in in-memory transport container
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> AdditionalRegistrations { get; set; }
            = Array.Empty<IManualRegistration>();
    }
}