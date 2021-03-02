namespace SpaceEngineers.Core.GenericEndpoint.Defaults
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    /// <summary>
    /// Default IRetryPolicy implementation
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
    public class DefaultRetryPolicy : IRetryPolicy
    {
        private static readonly int[] Scale = new[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };

        /// <inheritdoc />
        public Task Apply(IExtendedIntegrationContext context, CancellationToken token)
        {
            var actualCounter = context.Message.ReadRetryCounter();

            if (actualCounter < Scale.Length)
            {
                var dueTime = TimeSpan.FromSeconds(Scale[actualCounter]);
                return context.Retry(dueTime, token);
            }

            // TODO: move message to error queue
            return Task.CompletedTask;
        }
    }
}