namespace SpaceEngineers.Core.TracingEndpoint.Repositories
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Domain;

    [Component(EnLifestyle.Scoped)]
    internal class CapturedMessageReadRepository : ICapturedMessageReadRepository
    {
        public IEnumerable<CapturedMessage> Read(Guid conversationId)
        {
            throw new NotImplementedException("#112");
        }
    }
}