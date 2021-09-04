namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Basics;
    using CompositionRoot;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.Contract.Abstractions;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Linq.Abstractions;
    using GenericDomain.Api;
    using GenericDomain.Api.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DataAccess assemblies test
    /// </summary>
    public class DataAccessTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DataAccessTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// DataAccessTestData member
        /// </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object> DataAccessTestData()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Dynamic))),
            };

            var emptyQueryParameters = new Dictionary<string, object?>();

            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - All",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison !=",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.IntField != 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.IntField)}\" != @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison <",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.IntField < 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.IntField)}\" < @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison <=",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.IntField <= 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.IntField)}\" <= @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison ==",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.IntField == 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.IntField)}\" = @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison >",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.IntField > 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.IntField)}\" > @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison >=",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.IntField >= 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.IntField)}\" >= @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary filter",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => it.NullableStringField).Where(it => it != null)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.NullableStringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Boolean property filter after anonymous projection",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => new { it.BooleanField, it.StringField }).Where(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.BooleanField)}\",{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.BooleanField)}\"",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Boolean property filter",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.BooleanField)}\"",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Change anonymous projection parameter name",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => new { it.NullableStringField, it.StringField }).Where(it => it.NullableStringField != null)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.NullableStringField)}\",{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Coalesce projection",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => it.NullableStringField ?? string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\tCOALESCE(a.\"{nameof(TestAggregate.NullableStringField)}\", @param_0){Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        new Dictionary<string, object?> { ["param_0"] = string.Empty },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`2 - anonymous class key test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().GroupBy(it => new { it.StringField, it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $"SELECT DISTINCT{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\ta.\"{nameof(TestAggregate.BooleanField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`2 - projection source test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.IntField >= 42).Select(it => new { it.StringField, it.BooleanField }).GroupBy(it => new { it.StringField, it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $"SELECT DISTINCT{Environment.NewLine}\tc.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\tc.\"{nameof(TestAggregate.BooleanField)}\"{Environment.NewLine}FROM{Environment.NewLine}\t(SELECT{Environment.NewLine}\t\tb.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\t\tb.\"{nameof(TestAggregate.BooleanField)}\"{Environment.NewLine}\tFROM{Environment.NewLine}\t\t(SELECT{Environment.NewLine}\t\t\t*{Environment.NewLine}\t\tFROM{Environment.NewLine}\t\t\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}\t\tWHERE{Environment.NewLine}\t\t\ta.\"{nameof(TestAggregate.IntField)}\" >= @param_0) b) c",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`2 - single field key test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().GroupBy(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $"SELECT DISTINCT{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`3 - anonymous class key test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().GroupBy(it => new { it.StringField, it.BooleanField }, it => new { it.IntField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $"SELECT DISTINCT{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\ta.\"{nameof(TestAggregate.BooleanField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`3 - projection source test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.IntField >= 42).Select(it => new { it.StringField, it.BooleanField, it.IntField }).GroupBy(it => new { it.StringField, it.BooleanField }, it => new { it.IntField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $"SELECT DISTINCT{Environment.NewLine}\tc.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\tc.\"{nameof(TestAggregate.BooleanField)}\"{Environment.NewLine}FROM{Environment.NewLine}\t(SELECT{Environment.NewLine}\t\tb.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\t\tb.\"{nameof(TestAggregate.BooleanField)}\",{Environment.NewLine}\t\tb.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}\tFROM{Environment.NewLine}\t\t(SELECT{Environment.NewLine}\t\t\t*{Environment.NewLine}\t\tFROM{Environment.NewLine}\t\t\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}\t\tWHERE{Environment.NewLine}\t\t\ta.\"{nameof(TestAggregate.IntField)}\" >= @param_0) b) c",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`3 - single field key test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().GroupBy(it => it.StringField, it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $"SELECT DISTINCT{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - One property projection - bool",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.BooleanField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - One property projection - guid",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => it.Id)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.Id)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - One property projection - int",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - One property projection - string",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Projection/filter chain",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => new { it.NullableStringField, it.StringField, it.IntField }).Select(it => new { it.NullableStringField, it.IntField }).Where(it => it.NullableStringField != null).Select(it => new { it.IntField }).Where(it => it.IntField > 0).Where(it => it.IntField < 42).Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\td.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}FROM{Environment.NewLine}\t(SELECT{Environment.NewLine}\t\tc.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}\tFROM{Environment.NewLine}\t\t(SELECT{Environment.NewLine}\t\t\tb.\"{nameof(TestAggregate.NullableStringField)}\",{Environment.NewLine}\t\t\tb.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}\t\tFROM{Environment.NewLine}\t\t\t(SELECT{Environment.NewLine}\t\t\t\ta.\"{nameof(TestAggregate.NullableStringField)}\",{Environment.NewLine}\t\t\t\ta.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\t\t\t\ta.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}\t\t\tFROM{Environment.NewLine}\t\t\t\tpublic.\"{nameof(TestAggregate)}\" a) b{Environment.NewLine}\t\tWHERE{Environment.NewLine}\t\t\tb.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL) c{Environment.NewLine}\tWHERE{Environment.NewLine}\t\tc.\"{nameof(TestAggregate.IntField)}\" > @param_0 AND c.\"{nameof(TestAggregate.IntField)}\" < @param_1) d",
                        new Dictionary<string, object?> { ["param_0"] = 0, ["param_1"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Property chain with translated member",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => it.StringField.Length)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\tlength(a.\"{nameof(TestAggregate.StringField)}\"){Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Ternary filter after projection with renaming",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => new { it.StringField, Filter = it.NullableStringField }).Where(it => it.Filter != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\ta.\"{nameof(TestAggregate.NullableStringField)}\" AS Filter{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\tCASE WHEN a.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Ternary filter after projection",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => new { it.StringField, it.NullableStringField }).Where(it => it.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\ta.\"{nameof(TestAggregate.NullableStringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\tCASE WHEN a.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Ternary filter",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(it => it.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\tCASE WHEN a.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Ternary projection",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(it => it.NullableStringField != null ? it.NullableStringField : string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\tCASE WHEN a.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL THEN a.\"{nameof(TestAggregate.NullableStringField)}\" ELSE @param_0 END{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        new Dictionary<string, object?> { ["param_0"] = string.Empty },
                        write))
            };
        }

        [Fact]
        internal void NextLambdaParameterNameTest()
        {
            var ctx = new TranslationContext();

            Assert.Equal("a", ctx.NextLambdaParameterName());
            Assert.Equal("b", ctx.NextLambdaParameterName());
            Assert.Equal("c", ctx.NextLambdaParameterName());

            Enumerable
                .Range(0, 42)
                .Each(_ => ctx.NextLambdaParameterName());

            Assert.Equal("at", ctx.NextLambdaParameterName());
            Assert.Equal("au", ctx.NextLambdaParameterName());
            Assert.Equal("av", ctx.NextLambdaParameterName());

            Enumerable
                .Range(0, 42)
                .Each(_ => ctx.NextLambdaParameterName());

            Assert.Equal("cm", ctx.NextLambdaParameterName());
            Assert.Equal("cn", ctx.NextLambdaParameterName());
            Assert.Equal("co", ctx.NextLambdaParameterName());
        }

        [Theory]
        [MemberData(nameof(DataAccessTestData))]
        internal void ReadRepositoryTest(
            string section,
            Assembly[] assemblies,
            Func<IReadRepository<TestAggregate>, IQueryable> queryProducer,
            Action<IQuery, Action<string>> checkQuery)
        {
            Output.WriteLine(section);
            Output.WriteLine(string.Empty);

            var options = new DependencyContainerOptions();
            var dependencyContainer = Fixture.BoundedAboveContainer(options, assemblies);
            var token = CancellationToken.None;

            using (dependencyContainer.OpenScope())
            {
                var readRepository = dependencyContainer
                    .Resolve<IReadRepository<TestAggregate>>();

                var query = dependencyContainer
                    .Resolve<IQueryTranslator>()
                    .Translate(queryProducer(readRepository).Expression, token)
                    .Result;

                checkQuery(query, Output.WriteLine);

                /* TODO: #135 - setup appveyor environment so as to run queries */
                /*var result = queryProducer(readRepository)
                    .GetEnumerator()
                    .AsEnumerable<object>()
                    .ToList();

                foreach (var @object in result)
                {
                    if (@object.GetType().IsSubclassOfOpenGeneric(typeof(IGrouping<,>))
                        && query is GroupedQuery groupedQuery)
                    {
                        var keyValues = @object
                            .GetPropertyValue(nameof(IGrouping<object, object>.Key))
                            .GetQueryParametersValues();

                        Output.WriteLine("Actual key values:");
                        Output.WriteLine(keyValues.Select(pair => pair.ToString()).ToString(Environment.NewLine));
                        Output.WriteLine(string.Empty);

                        var valuesExpression = groupedQuery.ValuesExpressionProducer.Invoke(keyValues);
                        var valuesQuery = valuesExpression.Translate(dependencyContainer, 0, token).Result;
                        var valuesQueryParameters = valuesExpression
                            .ExtractQueryParameters(dependencyContainer)
                            .GetQueryParametersValues();

                        Output.WriteLine("Actual values query parameters:");
                        Output.WriteLine(valuesQueryParameters.Select(pair => pair.ToString()).ToString(Environment.NewLine));
                        Output.WriteLine(string.Empty);

                        Output.WriteLine("Actual values query:");
                        Output.WriteLine(valuesQuery);
                        Output.WriteLine(string.Empty);

                        Output.WriteLine("Actual values:");

                        var enumerator = ((IEnumerable)@object).GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            Output.WriteLine(enumerator.Current.ToString());
                        }
                    }
                    else
                    {
                        Output.WriteLine(@object.ToString());
                    }

                    Output.WriteLine(string.Empty);
                }*/

                /* TODO: #134 - IAsyncQueryable extensions
                _ = await readRepository
                    .All()
                    .Select(entity => entity.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
                */
            }
        }

        private static void CheckFlatQuery(
            IQuery query,
            string expectedQuery,
            IReadOnlyDictionary<string, object?> expectedQueryParameters,
            Action<string> write)
        {
            var flatQuery = (FlatQuery)query;

            write("Expected query:");
            write(expectedQuery);

            write(string.Empty);

            write("Actual query:");
            write(flatQuery.Query);

            Assert.Equal(expectedQuery, flatQuery.Query, StringComparer.Ordinal);
            CheckParameters(flatQuery.QueryParameters, expectedQueryParameters);
        }

        private static void CheckGroupedQuery(
            IQuery query,
            string expectedKeysQuery,
            IReadOnlyDictionary<string, object?> expectedKeysQueryParameters,
            Action<string> write)
        {
            var groupedQuery = (GroupedQuery)query;

            write("Expected keys query:");
            write(expectedKeysQuery);

            write(string.Empty);

            write("Actual keys query:");
            write(groupedQuery.KeysQuery);

            Assert.Equal(expectedKeysQuery, groupedQuery.KeysQuery, StringComparer.Ordinal);
            CheckParameters(groupedQuery.KeysQueryParameters, expectedKeysQueryParameters);
            write(string.Empty);
        }

        private static void CheckParameters(object? actualQueryParameters, IReadOnlyDictionary<string, object?> expectedQueryParameters)
        {
            var parameters = (actualQueryParameters?.ToPropertyDictionary() ?? new Dictionary<string, object?>())
                .FullOuterJoin(expectedQueryParameters,
                    actual => actual.Key,
                    expected => expected.Key,
                    (actual, expected) => (actual, expected),
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            var wrongPairs = parameters
                .Where(parameter => Equals(parameter.actual, default(KeyValuePair<string, object?>))
                                    || Equals(parameter.expected, default(KeyValuePair<string, object?>)));

            Assert.Empty(wrongPairs);

            foreach (var (actual, expected) in parameters)
            {
                Assert.Equal(expected.Value, actual.Value);
            }
        }

        internal class TestAggregate : EntityBase, IAggregate
        {
            public TestAggregate(bool booleanField, string stringField)
            {
                BooleanField = booleanField;
                StringField = stringField;
            }

            public bool BooleanField { get; private set; }

            public string StringField { get; private set; }

            public string? NullableStringField { get; set; }

            public int IntField { get; set; }
        }
    }
}