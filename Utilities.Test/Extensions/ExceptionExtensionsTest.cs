namespace SpaceEngineers.Core.Utilities.Test
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class ExceptionExtensionsTest : TestBase
    {
        public ExceptionExtensionsTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ExceptionExtensionsUnwrapTest()
        {
            var inner1 = new Exception("#1");
            var inner2 = new Exception("#2");
            var inner3 = new Exception("#3");

            var wrap1 = new AggregateException(inner2, inner3);
            var wrap2 = new TargetInvocationException(wrap1);
            
            var sourceException = new AggregateException(inner1, wrap1, wrap2, inner2, inner3);

            var result = sourceException.Extract().ToArray();

            Output.WriteLine(string.Join(", ", result.Select(z => z.Message)));

            Assert.Contains(inner1, result);
            Assert.Contains(inner2, result);
            Assert.Contains(inner3, result);
            
            Assert.DoesNotContain(sourceException, result);
            Assert.DoesNotContain(wrap1, result);
            Assert.DoesNotContain(wrap2, result);
            
            Assert.Equal(1, result.Count(ex => ex == inner1));
            Assert.Equal(1, result.Count(ex => ex == inner2));
            Assert.Equal(1, result.Count(ex => ex == inner3));
        }
    }
}