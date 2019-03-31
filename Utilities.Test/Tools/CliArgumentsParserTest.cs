namespace SpaceEngineers.Core.Utilities.Test
{
    using System;
    using System.Reflection;
    using Extensions;
    using Tools;
    using Xunit;
    using Xunit.Abstractions;

    public class CliArgumentsParserTest : TestBase
    {
        public CliArgumentsParserTest(ITestOutputHelper output)
            : base(output) { }

        #pragma warning disable xUnit2000 // Constants and literals should be the expected argument
        
        [Fact]
        public void Test1()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-bool",
                                                    "-nullablebool",
                                                    "-testenum=value1",
                                                    "-nullabletestenum=Value2"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test2()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-bool=true",
                                                    "-nullablebool=true",
                                                    "-testenum=Value1",
                                                    "-nullabletestenum=value2"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test4()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-string"
                                                });

            var arguments = parser.Parse<TestPoco>();

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
        public void Test5()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-string=qwerty"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
            Assert.Equal(arguments.Bool, false);
            Assert.Equal(arguments.NullableBool, null);
            Assert.Equal(arguments.TestEnum, TestEnum.Default);
            Assert.Equal(arguments.NullableTestEnum, null);
            Assert.Equal(arguments.TestFlagsEnum, TestFlagsEnum.Default);
            Assert.Equal(arguments.NullableTestFlagsEnum, null);
            Assert.Equal(arguments.String, "qwerty");
        }

        [Fact]
        public void Test6()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-string=\"qwerty qwerty\""
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test7()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-string=\"https://www.google.com\""
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test8()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-string=https://www.google.com"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test9()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-string='https://www.yandex.ru;https://www.google.com'"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test10()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-TestFlagsEnum=Value1",
                                                    "-NullableTestFlagsEnum=value2"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test11()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-TestFlagsEnum=value1",
                                                    "-NullableTestFlagsEnum=value2;VaLuE1"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test12()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CliArgumentsParser(new[]
                                                                                   {
                                                                                       "-bool",
                                                                                       "-bool"
                                                                                   }));

            Assert.Contains("'bool' already added", ex.Message);
        }

        [Fact]
        public void Test13()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-nullablebool",
                                                    "-TestFlagsEnum=value3",
                                                    "-bool"
                                                });

            var ex = Assert.Throws<ArgumentException>(() => parser.Parse<TestPoco>());
            Assert.Contains("Value 'value3' is not recognized", ex.Message);
        }
        
        [Fact]
        public void Test14()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-nullablebool",
                                                    "-NullableTestFlagsEnum=\"value2 ; VaLuE1\"",
                                                    "-bool"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test15()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-nullablebool",
                                                    "-NullableTestFlagsEnum=value2 ; VaLuE1",
                                                    "-bool"
                                                });

            var arguments = parser.Parse<TestPoco>();
            
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
        public void Test16()
        {
            var parser = new CliArgumentsParser(new[]
                                                {
                                                    "-nullablebool",
                                                    "-NullableTestFlagsEnum=value2;value3",
                                                    "-bool"
                                                });

            var ex = Assert.Throws<ArgumentException>(() => parser.Parse<TestPoco>());
            Assert.Contains("Values 'value3' is not recognized", ex.Message);
        }
        
        #pragma warning restore xUnit2000 // Constants and literals should be the expected argument

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
            public string String { get; set; }

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