namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Api.Abstractions;
    using CrossCuttingConcerns.Json;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// SerializationTest
    ///     Json module
    /// </summary>
    public class SerializationTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public SerializationTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
            var assembly = typeof(IJsonSerializer).Assembly; // CrossCuttingConcerns

            DependencyContainer = fixture.BoundedAboveContainer(new DependencyContainerOptions(), assembly);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void ObjectTreeDeserializationTest()
        {
            var serialized = @"{
""candles"": {
        ""metadata"": {
                ""open"": {""type"": ""double""},
                ""close"": {""type"": ""double""},
                ""high"": {""type"": ""double""},
                ""low"": {""type"": ""double""},
                ""value"": {""type"": ""double""},
                ""volume"": {""type"": ""double""},
                ""begin"": {""type"": ""datetime"", ""bytes"": 19, ""max_size"": 0},
                ""end"": {""type"": ""datetime"", ""bytes"": 19, ""max_size"": 0}
        },
        ""columns"": [""open"", ""close"", ""high"", ""low"", ""value"", ""volume"", ""begin"", ""end""],
        ""data"": [
                [82.14, 82.14, 82.14, 82.14, 4930864.2, 60030, ""2020-09-16 09:00:00"", ""2020-09-16 09:59:59""],
                [82.38, 83.14, 83.76, 82.24, 204154615.8, 2456970, ""2020-09-16 10:00:00"", ""2020-09-16 10:59:59""],
                [83.14, 83.08, 83.36, 82.92, 52378085.99999999, 630140, ""2020-09-16 11:00:00"", ""2020-09-16 11:59:59""],
                [83.06, 82.96, 83.14, 82.9, 33313677.200000003, 401210, ""2020-09-16 12:00:00"", ""2020-09-16 12:59:59""],
                [82.96, 82.92, 83.1, 82.86, 38844514.79999998, 468140, ""2020-09-16 13:00:00"", ""2020-09-16 13:59:59""],
                [82.94, 83.04, 83.04, 82.84, 20225836.19999999, 243760, ""2020-09-16 14:00:00"", ""2020-09-16 14:59:59""],
                [83.06, 82.86, 83.12, 82.7, 25191517.6, 303770, ""2020-09-16 15:00:00"", ""2020-09-16 15:59:59""],
                [82.94, 82.62, 83, 82.54, 46140851.6, 557740, ""2020-09-16 16:00:00"", ""2020-09-16 16:59:59""],
                [82.64, 82.78, 82.96, 82.52, 42417728.39999998, 512440, ""2020-09-16 17:00:00"", ""2020-09-16 17:59:59""],
                [82.84, 82.9, 82.9, 82.62, 18994727.8, 229340, ""2020-09-16 18:00:00"", ""2020-09-16 18:48:11""],
                [82.58, 82.86, 82.94, 82.58, 5006094.6, 60490, ""2020-09-16 19:00:00"", ""2020-09-16 19:59:59""],
                [82.86, 82.78, 83, 82.74, 14463095.8, 174470, ""2020-09-16 20:00:00"", ""2020-09-16 20:59:59""],
                [82.74, 82.68, 82.84, 82.64, 6486566.600000001, 78350, ""2020-09-16 21:00:00"", ""2020-09-16 21:59:59""]
        ]
}}";

            var serializer = DependencyContainer.Resolve<IJsonSerializer>();

            var node = serializer.DeserializeObject<IObjectTreeNode>(serialized);
            Assert.NotNull(node);

            var tree = node.ExtractTree() as IDictionary<string, object?>;
            Assert.NotNull(tree);
            Assert.Contains("candles", tree);

            var candles = tree["candles"] as IDictionary<string, object?>;
            Assert.NotNull(candles);
            Assert.Contains("data", candles);

            var rows = candles["data"] as ICollection<object?>;
            Assert.NotNull(rows);
            Assert.Equal(13, rows.Count);

            var row = rows.Last() as ICollection<object?>;
            Assert.NotNull(row);
            Assert.Equal(8, row.Count);

            var number = row.First() as double?;
            Assert.NotNull(number);
            Assert.Equal(82.74, number);

            var strDate = row.Last() as string;
            Assert.NotNull(strDate);
            var date = Convert.ToDateTime(strDate, CultureInfo.GetCultureInfo("en-US"));
            Assert.Equal(new DateTime(2020, 09, 16, 21, 59, 59), date);
        }
    }
}