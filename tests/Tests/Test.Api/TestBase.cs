namespace SpaceEngineers.Core.Test.Api;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using Basics;
using ClassFixtures;
using Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using TraceListener = Logging.TraceListener;

public abstract class TestBase : IClassFixture<TestFixture>,
    IDisposable
{
    internal static readonly AsyncLocal<TestBase?> Local = new AsyncLocal<TestBase?>();

    protected TestBase(ITestOutputHelper output, TestFixture fixture)
    {
        Output = output;
        Fixture = fixture;

        Local.Value ??= this;
    }

    public ITestOutputHelper Output { get; }

    public TestFixture Fixture { get; }

    public IXunitTestCase TestCase => (IXunitTestCase)Output.GetFieldValue<ITest>("test").TestCase;

    [SuppressMessage("Analysis", "CA2000", Justification = "IDbConnection will be disposed in outer scope by client")]
    public static void Redirect()
    {
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new TraceListener());

        Type.GetType("System.Diagnostics.DebugProvider")
            .GetField("s_WriteCore", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, DebugListener.Write);

        var writer = new TestOutputTextWriter();
        Console.SetOut(writer);
        Console.SetError(writer);
    }

    public void Dispose()
    {
        Local.Value = null;
    }
}