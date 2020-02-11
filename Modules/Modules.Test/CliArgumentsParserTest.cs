namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Basics;
    using CliArgumentsParser;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// CliArgumentsParser class tests
    /// </summary>
    public class CliArgumentsParserTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public CliArgumentsParserTest(ITestOutputHelper output)
            : base(output) { }

        #pragma warning disable xUnit2000 // Constants and literals should be the expected argument

        [Fact]
        internal void Test1()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-bool",
                           "-nullablebool",
                           "-testenum=value1",
                           "-nullabletestenum=Value2"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, true);
            Assert.Equal(arguments.NullableBool, true);
            Assert.Equal(arguments.TestEnum, TestEnum.Value1);
            Assert.Equal(arguments.NullableTestEnum, TestEnum.Value2);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, null);
        }

        [Fact]
        internal void Test2()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-bool=true",
                           "-nullablebool=true",
                           "-testenum=Value1",
                           "-nullabletestenum=value2"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, true);
            Assert.Equal(arguments.NullableBool, true);
            Assert.Equal(arguments.TestEnum, TestEnum.Value1);
            Assert.Equal(arguments.NullableTestEnum, TestEnum.Value2);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, null);
        }

        [Fact]
        internal void Test4()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-string"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, null);
        }

        [Fact]
        internal void Test5()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-string=qwerty"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, "qwerty");
        }

        [Fact]
        internal void Test6()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-string=\"qwerty qwerty\""
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, "qwerty qwerty");
        }

        [Fact]
        internal void Test7()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-string=\"https://www.google.com\""
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, "https://www.google.com");
        }

        [Fact]
        internal void Test8()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-string=https://www.google.com"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, "https://www.google.com");
        }

        [Fact]
        internal void Test9()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-string='https://www.yandex.ru;https://www.google.com'"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, "https://www.yandex.ru;https://www.google.com");
        }

        [Fact]
        internal void Test10()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-TestFlagsEnum=Value1",
                           "-NullableTestFlagsEnum=value2"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Value1);
            Assert.Equal(arguments.NullableTestFlagsEnum, TestFlagsEnum.Value2);
            Assert.Equal(arguments.String, null);
        }

        [Fact]
        internal void Test11()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-TestFlagsEnum=value1",
                           "-NullableTestFlagsEnum=value2;VaLuE1"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Value1);
            Assert.Equal(arguments.NullableTestFlagsEnum, TestFlagsEnum.Value1 | TestFlagsEnum.Value2);
            Assert.Equal(arguments.String, null);
        }

        [Fact]
        internal void Test12()
        {
            var args = new[]
                       {
                           "-bool",
                           "-bool"
                       };

            var ex = Assert.Throws<ArgumentException>(() => DependencyContainer.Resolve<ICliArgumentsParser>().Parse<TestPoco>(args));

            Assert.Contains("'bool' already added", ex.Message, StringComparison.InvariantCulture);
        }

        [Fact]
        internal void Test13()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-nullablebool",
                           "-TestFlagsEnum=value3",
                           "-bool"
                       };

            var ex = Assert.Throws<ArgumentException>(() => parser.Parse<TestPoco>(args));
            Assert.Contains("Value 'value3' is not recognized", ex.Message, StringComparison.InvariantCulture);
        }

        [Fact]
        internal void Test14()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-nullablebool",
                           "-NullableTestFlagsEnum=\"value2 ; VaLuE1\"",
                           "-bool"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, true);
            Assert.Equal(arguments.NullableBool, true);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, TestFlagsEnum.Value1 | TestFlagsEnum.Value2);
            Assert.Equal(arguments.String, null);
        }

        [Fact]
        internal void Test15()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-nullablebool",
                           "-NullableTestFlagsEnum=value2 ; VaLuE1",
                           "-bool"
                       };

            var arguments = parser.Parse<TestPoco>(args);

            Output.WriteLine(arguments.ToString());

            Assert.Equal(arguments.Bool, true);
            Assert.Equal(arguments.NullableBool, true);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, TestFlagsEnum.Value1 | TestFlagsEnum.Value2);
            Assert.Equal(arguments.String, null);
        }

        [Fact]
        internal void Test16()
        {
            var parser = DependencyContainer.Resolve<ICliArgumentsParser>();

            var args = new[]
                       {
                           "-nullablebool",
                           "-NullableTestFlagsEnum=value2;value3",
                           "-bool"
                       };

            var ex = Assert.Throws<ArgumentException>(() => parser.Parse<TestPoco>(args));
            Assert.Contains("Values 'value3' is not recognized", ex.Message, StringComparison.InvariantCulture);
        }

        #pragma warning restore xUnit2000 // Constants and literals should be the expected argument

        [SuppressMessage("StyleCop.Analyzers", "SA1201", Justification = "For test reasons")]
        private enum TestEnum
        {
            Default,
            Value1,
            Value2
        }

        [Flags]
        private enum TestFlagsEnum
        {
            Default,
            Value1,
            Value2
        }

        private class TestPoco
        {
            /// <summary> Bool </summary>
            public bool Bool { get; set; }

            /// <summary> NullableBool </summary>
            public bool? NullableBool { get; set; }

            /// <summary> TestEnum </summary>
            public TestEnum TestEnum { get; set; }

            /// <summary> NullableTestEnum </summary>
            public TestEnum? NullableTestEnum { get; set; }

            /// <summary> TestFlagsEnum </summary>
            public TestFlagsEnum TestFlagsEnum { get; set; }

            /// <summary> NullableTestFlagsEnum </summary>
            public TestFlagsEnum? NullableTestFlagsEnum { get; set; }

            /// <summary> String </summary>
            public string? String { get; set; } = null;

            /// <inheritdoc />
            public override string ToString()
            {
                return this.ShowProperties(BindingFlags.Public
                                           | BindingFlags.Instance
                                           | BindingFlags.GetProperty
                                           | BindingFlags.SetProperty);
            }
        }
    }
}