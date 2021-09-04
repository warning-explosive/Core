namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using Basics;

    internal class MessageHandlerDoesNotThrowExceptionTestCase<TException> : ITestCase
        where TException : Exception
    {
        private const string Format = "Message handler doesn't have to throw any exception with type {0}";

        public string? Assert(TestIntegrationContext integrationContext, Exception? exception)
        {
            return exception is not TException
                ? null
                : Format.Format(typeof(TException).FullName);
        }
    }
}