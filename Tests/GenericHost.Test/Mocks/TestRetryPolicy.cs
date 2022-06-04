namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class TestRetryPolicy : IRetryPolicy,
                                     IResolvable<IRetryPolicy>
    {
        private static readonly int[] Scale = new[] { 0, 1, 2 };

        public Task Apply(IAdvancedIntegrationContext context, Exception exception, CancellationToken token)
        {
            var actualCounter = context.Message.ReadHeader<RetryCounter>()?.Value ?? 0;

            if (actualCounter < Scale.Length)
            {
                var dueTime = TimeSpan.FromSeconds(Scale[actualCounter]);
                return context.Retry(dueTime, token);
            }

            return context.Reject(exception, token);
        }
    }
}