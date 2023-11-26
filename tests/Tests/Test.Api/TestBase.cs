namespace SpaceEngineers.Core.Test.Api
{
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

    /// <summary>
    /// TestBase
    /// </summary>
    public abstract class TestBase : IClassFixture<TestFixture>,
                                     IDisposable
    {
        internal static readonly AsyncLocal<TestBase?> Local = new AsyncLocal<TestBase?>();

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        protected TestBase(ITestOutputHelper output, TestFixture fixture)
        {
            Output = output;
            Fixture = fixture;

            Local.Value ??= this;
        }

        /// <summary>
        /// ITestOutputHelper
        /// </summary>
        public ITestOutputHelper Output { get; }

        /// <summary>
        /// TestFixture
        /// </summary>
        public TestFixture Fixture { get; }

        /// <summary>
        /// TestCase
        /// </summary>
        public IXunitTestCase TestCase => (IXunitTestCase)Output.GetFieldValue<ITest>("test").TestCase;

        /// <summary>
        /// Redirects outputs to xUnit ITestOutputHelper
        /// </summary>
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

        /// <inheritdoc />
        public void Dispose()
        {
            Local.Value = null;
        }
    }
}