namespace SpaceEngineers.Core.TracingEndpoint.Repositories
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using Domain;

    internal interface ICapturedMessageReadRepository : IResolvable
    {
        public IEnumerable<CapturedMessage> Read(Guid conversationId);
    }
}