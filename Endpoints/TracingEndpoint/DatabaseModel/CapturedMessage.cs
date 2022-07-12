namespace SpaceEngineers.Core.TracingEndpoint.DatabaseModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using DataAccess.Api.Model;

    [SuppressMessage("Analysis", "SA1649", Justification = "StyleCop analyzer error")]
    [Schema(nameof(GenericEndpoint.Tracing))]
    internal record CapturedMessage : BaseDatabaseEntity<Guid>
    {
        public CapturedMessage(
            Guid primaryKey,
            IntegrationMessage message,
            string? refuseReason)
            : base(primaryKey)
        {
            Message = message;
            RefuseReason = refuseReason;
        }

        public IntegrationMessage Message { get; init; }

        public string? RefuseReason { get; init; }
    }
}