namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using Basics;

    internal class MessageHandlerThrowsExceptionTestCase<TException> : ITestCase
        where TException : Exception
    {
        private const string Format = "Message handler has to throw exception with type {0}";

        private readonly Func<TException, bool> _assertion;

        public MessageHandlerThrowsExceptionTestCase(Func<TException, bool> assertion)
        {
            _assertion = assertion;
        }

        public string? Assert(ITestIntegrationContext integrationContext, Exception? exception)
        {
            return exception is TException exactException && _assertion(exactException)
                ? null
                : Format.Format(typeof(TException).FullName);
        }
    }
}