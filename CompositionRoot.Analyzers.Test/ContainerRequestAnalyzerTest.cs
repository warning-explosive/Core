namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using Roslyn.Test;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ContainerRequestAnalyzer test
    /// </summary>
    public class ContainerRequestAnalyzerTest : DiagnosticAnalyzerTestBase<ContainerRequestAnalyzer>
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public ContainerRequestAnalyzerTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void ContainerRequestTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using System.Reflection;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;

    public interface ITestService : IResolvable, ICollectionResolvable
    {
    }

    internal class TestServiceImpl : ITestService
    {
        public void TestMethod()
        {
            var container = new DependencyContainer(Array.Empty<Assembly>());

            container.Resolve(typeof(ITestService));
            container.Resolve<ITestService>();

            container.ResolveCollection(typeof(ITestService));
            container.ResolveCollection<ITestService>();

            new DependencyContainer(Array.Empty<Assembly>()).Resolve(typeof(ITestService));
            new DependencyContainer(Array.Empty<Assembly>()).Resolve<ITestService>();

            new DependencyContainer(Array.Empty<Assembly>()).ResolveCollection(typeof(ITestService));
            new DependencyContainer(Array.Empty<Assembly>()).ResolveCollection<ITestService>();

            WithArgsTestMethod(container.Resolve(typeof(ITestService)));
            WithArgsTestMethod(container.Resolve<ITestService>());

            new TestDataClass { Property = container.Resolve(typeof(ITestService)) };
            new TestDataClass { Property = container.Resolve<ITestService>() };
        }

        public void WithArgsTestMethod(ITestService service)
        {
        }
    }

    internal class TestDataClass
    {
        public ITestService Property { get; set; }
    }
}";

            VerifyAnalyzer(test,
                           Expected(18, 13),
                           Expected(19, 13),
                           Expected(21, 13),
                           Expected(22, 13),
                           Expected(24, 13),
                           Expected(25, 13),
                           Expected(27, 13),
                           Expected(28, 13),
                           Expected(30, 32),
                           Expected(31, 32),
                           Expected(33, 44),
                           Expected(34, 44));
        }
    }
}