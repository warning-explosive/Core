namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// EnumerableExtensions test
    /// </summary>
    public class EnumerableExtensionsTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public EnumerableExtensionsTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void SimpleColumnsCartesianProductTest()
        {
            IEnumerable<IEnumerable<Type>> columns = new List<IEnumerable<Type>>
                                                     {
                                                         new List<Type>
                                                         {
                                                             typeof(object),
                                                             typeof(bool),
                                                             typeof(string),
                                                             typeof(Enum)
                                                         }
                                                     };

            IEnumerable<IEnumerable<Type>> expected = new List<IEnumerable<Type>>
                                                      {
                                                          new List<Type>
                                                          {
                                                              typeof(object)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(bool)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(string)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(Enum)
                                                          }
                                                      };

            Assert.True(CheckEquality(expected, columns.ColumnsCartesianProduct()));

            columns = new List<IEnumerable<Type>>
                      {
                          new List<Type>
                          {
                              typeof(object),
                              typeof(bool)
                          },
                          new List<Type>
                          {
                              typeof(string),
                              typeof(Enum)
                          }
                      };

            expected = new List<IEnumerable<Type>>
                       {
                           new List<Type>
                           {
                               typeof(object),
                               typeof(string)
                           },
                           new List<Type>
                           {
                               typeof(object),
                               typeof(Enum)
                           },
                           new List<Type>
                           {
                               typeof(bool),
                               typeof(string)
                           },
                           new List<Type>
                           {
                               typeof(bool),
                               typeof(Enum)
                           }
                       };

            Assert.True(CheckEquality(expected, columns.ColumnsCartesianProduct()));
        }

        [Fact]
        internal void ComplexColumnsCartesianProductTest()
        {
            IEnumerable<IEnumerable<Type>> columns = new List<IEnumerable<Type>>
                                                     {
                                                         new List<Type>
                                                         {
                                                             typeof(object),
                                                             typeof(bool)
                                                         },
                                                         new List<Type>
                                                         {
                                                             typeof(string),
                                                             typeof(Enum)
                                                         },
                                                         new List<Type>
                                                         {
                                                             typeof(int),
                                                             typeof(decimal)
                                                         }
                                                     };

            IEnumerable<IEnumerable<Type>> expected = new List<IEnumerable<Type>>
                                                      {
                                                          new List<Type>
                                                          {
                                                              typeof(object),
                                                              typeof(string),
                                                              typeof(int)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(object),
                                                              typeof(string),
                                                              typeof(decimal)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(object),
                                                              typeof(Enum),
                                                              typeof(int)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(object),
                                                              typeof(Enum),
                                                              typeof(decimal)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(bool),
                                                              typeof(string),
                                                              typeof(int)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(bool),
                                                              typeof(string),
                                                              typeof(decimal)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(bool),
                                                              typeof(Enum),
                                                              typeof(int)
                                                          },
                                                          new List<Type>
                                                          {
                                                              typeof(bool),
                                                              typeof(Enum),
                                                              typeof(decimal)
                                                          }
                                                      };

            Assert.True(CheckEquality(expected, columns.ColumnsCartesianProduct()));
        }

        [Fact]
        internal void EmptyColumnsCartesianProductTest()
        {
            IEnumerable<IEnumerable<Type>> columns = Enumerable.Empty<IEnumerable<Type>>();
            IEnumerable<IEnumerable<Type>> expected = Enumerable.Empty<IEnumerable<Type>>();

            Assert.True(CheckEquality(expected, columns.ColumnsCartesianProduct()));

            columns = new List<IEnumerable<Type>>
                      {
                          new List<Type>
                          {
                              typeof(object),
                              typeof(bool)
                          },
                          new List<Type>
                          {
                              typeof(string),
                              typeof(Enum)
                          },
                          Enumerable.Empty<Type>()
                      };

            Assert.True(CheckEquality(expected, columns.ColumnsCartesianProduct()));
        }

        private bool CheckEquality(IEnumerable<IEnumerable<Type>> expected, IEnumerable<IEnumerable<Type>> actual)
        {
            Showx(actual);

            return expected.Count() == actual.Count()
                && expected.Zip(actual).All(pair => pair.First.SequenceEqual(pair.Second));
        }

        private void Showx(IEnumerable<IEnumerable<Type>> source) // Todo
        {
            foreach (var row in source)
            {
                foreach (var item in row)
                {
                    Output.WriteLine(item.Name);
                }

                Output.WriteLine("-*-*-*-*-*-");
            }
        }
    }
}