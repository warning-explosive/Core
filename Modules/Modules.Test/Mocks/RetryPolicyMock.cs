namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Messaging;

    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.Override)]
    internal class RetryPolicyMock : IRetryPolicy
    {
        private static readonly int[] Scale = new[] { 0, 1, 2 };

        /// <inheritdoc />
        public Task Apply(IAdvancedIntegrationContext context, Exception exception, CancellationToken token)
        {
            var actualCounter = context.Message.ReadHeader<int>(IntegrationMessageHeader.RetryCounter);

            if (actualCounter < Scale.Length)
            {
                var dueTime = TimeSpan.FromSeconds(Scale[actualCounter]);
                return context.Retry(dueTime, token);
            }

            return context.Refuse(exception, token);
        }
    }
}