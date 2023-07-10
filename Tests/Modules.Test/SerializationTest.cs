namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;
    using Basics;
    using CompositionRoot;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
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
        /// <param name="fixture">TestFixture</param>
        public SerializationTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory not found");

            var settingsDirectory = projectFileDirectory.StepInto("Settings");

            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns)));

            var options = new DependencyContainerOptions()
                .WithManualRegistrations(new SettingsDirectoryProviderManualRegistration(new SettingsDirectoryProvider(settingsDirectory)));

            DependencyContainer = fixture.BoundedAboveContainer(output, options, assembly);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void TypeSerializationTest()
        {
            var serializer = DependencyContainer.Resolve<IJsonSerializer>();

            var serialized = serializer.SerializeObject(typeof(object), typeof(Type));
            Output.WriteLine(serialized);
            Assert.Equal(typeof(object), serializer.DeserializeObject<Type>(serialized));

            serialized = serializer.SerializeObject(TypeNode.FromType(typeof(object)), typeof(TypeNode));
            Output.WriteLine(serialized);
            Assert.Equal(typeof(object), TypeNode.ToType(serializer.DeserializeObject<TypeNode>(serialized)));
        }

        [Fact]
        internal void PolymorphicSerializationTest()
        {
            var str = "qwerty";
            object obj = str;

            var payload = new
            {
                Str = str,
                Obj = obj
            };

            var serializer = DependencyContainer.Resolve<IJsonSerializer>();

            var serialized = serializer.SerializeObject(payload, payload.GetType());
            Output.WriteLine(serialized);
            var deserialized = serializer.DeserializeObject(serialized, payload.GetType());

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.GetPropertyValue("Str"));
            Assert.NotNull(deserialized.GetPropertyValue("Obj"));
        }

        [Theory]
        [InlineData($@"{{""Id"": 42}}")]
        [InlineData($@"{{""Id"": 42, ""$id"": ""1"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata""}}")]
        [InlineData($@"{{""$id"": ""1"", ""Id"": 42, ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata""}}")]
        [InlineData($@"{{""$id"": ""1"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata"", ""Id"": 42}}")]
        [InlineData($@"{{""Id"": 42, ""Inner"": {{""Id"": 43 }}}}")]
        [InlineData($@"{{""Id"": 42, ""Inner"": {{""Id"": 43 }}, ""$id"": ""1"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata""}}")]
        [InlineData($@"{{""$id"": ""1"", ""Id"": 42, ""Inner"": {{""Id"": 43 }}, ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata""}}")]
        [InlineData($@"{{""$id"": ""1"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata"", ""Id"": 42, ""Inner"": {{""Id"": 43 }}}}")]
        [InlineData($@"{{""Id"": 42, ""Inner"": {{""Id"": 43, ""$id"": ""2"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata"" }}}}")]
        [InlineData($@"{{""Id"": 42, ""Inner"": {{""Id"": 43, ""$id"": ""2"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata"" }}, ""$id"": ""1"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata""}}")]
        [InlineData($@"{{""$id"": ""1"", ""Id"": 42, ""Inner"": {{""Id"": 43, ""$id"": ""2"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata"" }}, ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata""}}")]
        [InlineData($@"{{""$id"": ""1"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata"", ""Id"": 42, ""Inner"": {{""Id"": 43, ""$id"": ""2"", ""$type"": ""SpaceEngineers.Core.Modules.Test SpaceEngineers.Core.Modules.Test.SerializationTest+TestMetadata"" }}}}")]
        internal void JsonMetadataDeserializationTest(string json)
        {
            Output.WriteLine(json);

            var obj = DependencyContainer
                .Resolve<IJsonSerializer>()
                .DeserializeObject<TestMetadata>(json);

            Assert.Equal(42, obj.Id);
        }

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

            var culture = CultureInfo.GetCultureInfo("en-US");
            var serializer = DependencyContainer.Resolve<IJsonSerializer>();

            var node = serializer.DeserializeObject<object>(serialized) as JsonObject;
            Assert.NotNull(node);

            Assert.True(node.ContainsKey("candles"));
            var candles = node["candles"] as JsonObject;
            Assert.NotNull(candles);

            Assert.True(candles.ContainsKey("data"));
            var rows = candles["data"] as JsonArray;
            Assert.NotNull(rows);
            Assert.Equal(13, rows.Count);

            var row = rows.Last() as JsonArray;
            Assert.NotNull(row);
            Assert.Equal(8, row.Count);

            var number = (row.First() as JsonValue)?.ToString();
            Assert.NotNull(number);
            Assert.Equal(82.74, double.Parse(number, culture));

            var strDate = (row.Last() as JsonValue)?.ToString();
            Assert.NotNull(strDate);
            Assert.Equal(new DateTime(2020, 09, 16, 21, 59, 59), Convert.ToDateTime(strDate, culture));
        }

        internal class TestMetadata
        {
            [JsonConstructor]
            public TestMetadata()
            {
            }

            public TestMetadata(
                int id,
                TestMetadata? inner)
            {
                Id = id;
                Inner = inner;
            }

            public int Id { get; init; }

            public TestMetadata? Inner { get; init; }
        }
    }
}