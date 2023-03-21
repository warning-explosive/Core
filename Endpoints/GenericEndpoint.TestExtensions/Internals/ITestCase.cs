namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;

    internal interface ITestCase
    {
        string? Assert(ITestIntegrationContext integrationContext, Exception? exception);
    }
}