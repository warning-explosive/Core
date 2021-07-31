namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.Contract.Abstractions;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Linq.Abstractions;
    using GenericDomain;
    using GenericDomain.Abstractions;
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
            };

            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - All",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\"",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - One property projection - guid",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.Id)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\te.\"{nameof(TestAggregate.Id)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - One property projection - bool",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\te.\"{nameof(TestAggregate.BooleanField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - One property projection - string",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\te.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - One property projection - int",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\te.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Boolean property filter",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\te.\"{nameof(TestAggregate.BooleanField)}\"",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Boolean property filter after anonymous projection",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => new { e.BooleanField, e.StringField }).Where(a => a.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.BooleanField)}\",{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.BooleanField)}\"",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Property chain",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.StringField.Length)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\tlength(e.\"{nameof(TestAggregate.StringField)}\"){Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary filter",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.NullableStringField).Where(str => str != null)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\te.\"{nameof(TestAggregate.NullableStringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\te.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Coalesce projection",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.NullableStringField ?? string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\tCOALESCE(e.\"{nameof(TestAggregate.NullableStringField)}\", ''){Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Ternary projection",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.NullableStringField != null ? e.NullableStringField : string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\tCASE WHEN e.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL THEN e.\"{nameof(TestAggregate.NullableStringField)}\" ELSE '' END{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Ternary filter",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\tCASE WHEN e.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL THEN True ELSE False END",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Ternary filter after projection",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => new { e.StringField, e.NullableStringField }).Where(p => p.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\tp.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\tp.\"{nameof(TestAggregate.NullableStringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" p{Environment.NewLine}WHERE{Environment.NewLine}\tCASE WHEN p.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL THEN True ELSE False END",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Ternary filter after projection with renaming",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => new { e.StringField, Filter = e.NullableStringField }).Where(p => p.Filter != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\tp.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\tp.\"{nameof(TestAggregate.NullableStringField)}\" AS Filter{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" p{Environment.NewLine}WHERE{Environment.NewLine}\tCASE WHEN p.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL THEN True ELSE False END",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison ==",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField == 0)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\te.\"{nameof(TestAggregate.IntField)}\" = 0",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison !=",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField != 0)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\te.\"{nameof(TestAggregate.IntField)}\" != 0",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison >=",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField >= 0)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\te.\"{nameof(TestAggregate.IntField)}\" >= 0",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison >",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField > 0)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\te.\"{nameof(TestAggregate.IntField)}\" > 0",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison <",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField < 0)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\te.\"{nameof(TestAggregate.IntField)}\" < 0",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Binary comparison <=",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField <= 0)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" e{Environment.NewLine}WHERE{Environment.NewLine}\te.\"{nameof(TestAggregate.IntField)}\" <= 0",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Change anonymous projection parameter name",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => new { e.NullableStringField, e.StringField }).Where(a => a.NullableStringField != null)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $"SELECT{Environment.NewLine}\ta.\"{nameof(TestAggregate.NullableStringField)}\",{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a{Environment.NewLine}WHERE{Environment.NewLine}\ta.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Projection/filter chain",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(a => new { a.NullableStringField, a.StringField, a.IntField }).Select(b => new { b.NullableStringField, b.IntField }).Where(c => c.NullableStringField != null).Select(d => new { d.IntField }).Where(e => e.IntField > 0).Where(f => f.IntField < 42).Select(g => g.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                    $"SELECT{Environment.NewLine}\tg.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}FROM{Environment.NewLine}\t(SELECT{Environment.NewLine}\t\tf.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}\tFROM{Environment.NewLine}\t\t(SELECT{Environment.NewLine}\t\t\tc.\"{nameof(TestAggregate.NullableStringField)}\",{Environment.NewLine}\t\t\tc.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}\t\tFROM{Environment.NewLine}\t\t\t(SELECT{Environment.NewLine}\t\t\t\ta.\"{nameof(TestAggregate.NullableStringField)}\",{Environment.NewLine}\t\t\t\ta.\"{nameof(TestAggregate.StringField)}\",{Environment.NewLine}\t\t\t\ta.\"{nameof(TestAggregate.IntField)}\"{Environment.NewLine}\t\t\tFROM{Environment.NewLine}\t\t\t\tpublic.\"{nameof(TestAggregate)}\" a) c{Environment.NewLine}\t\tWHERE{Environment.NewLine}\t\t\tc.\"{nameof(TestAggregate.NullableStringField)}\" IS NOT NULL) f{Environment.NewLine}\tWHERE{Environment.NewLine}\t\tf.\"{nameof(TestAggregate.IntField)}\" > 0 AND f.\"{nameof(TestAggregate.IntField)}\" < 42) g",
                    write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`2 - single field key test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().GroupBy(a => a.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $"SELECT DISTINCT{Environment.NewLine}\ta.\"{nameof(TestAggregate.StringField)}\"{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"{nameof(TestAggregate)}\" a",
                        $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\tpublic.\"TestAggregate\"{Environment.NewLine}WHERE{Environment.NewLine}\ttodo_key_predicate",
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`2 - anonymous class key test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().GroupBy(a => new { a.StringField, a.BooleanField })),
                new Action<IQuery, Action<string>>((query, write) => CheckGroupedQuery(query, string.Empty, string.Empty, write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - GroupBy`2 - projection source test",
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField > 42).Select(b => new { b.StringField, b.BooleanField }).GroupBy(a => new { a.StringField, a.BooleanField })),
                new Action<IQuery, Action<string>>((query, write) => CheckGroupedQuery(query, string.Empty, string.Empty, write))
            };
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

            var dependencyContainer = Fixture.BoundedAboveContainer(new DependencyContainerOptions(), assemblies);

            using (dependencyContainer.OpenScope())
            {
                var readRepository = dependencyContainer
                    .Resolve<IReadRepository<TestAggregate>>();

                var query = dependencyContainer
                    .Resolve<IQueryTranslator>()
                    .Translate(queryProducer(readRepository).Expression);

                checkQuery(query, Output.WriteLine);

                /* TODO: Prepare and assert queried data */
                var result = queryProducer(readRepository)
                    .GetEnumerator()
                    .ToObjectEnumerable()
                    .ToList();

                foreach (var @object in result)
                {
                    Output.WriteLine(@object.ToString());
                }

                /* TODO: IAsyncQueryable extensions
                _ = await readRepository
                    .All()
                    .Select(entity => entity.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
                */
            }
        }

        private static void CheckFlatQuery(IQuery query, string expectedQuery, Action<string> write)
        {
            var flatQuery = (FlatQuery)query;

            write("Expected query:");
            write(expectedQuery);

            write(string.Empty);

            write("Actual query:");
            write(flatQuery.Query);

            Assert.Equal(expectedQuery, flatQuery.Query, StringComparer.Ordinal);
        }

        private static void CheckGroupedQuery(IQuery query, string expectedKeysQuery, string expectedValuesQuery, Action<string> write)
        {
            var groupedQuery = (GroupedQuery)query;

            write("Expected keys query:");
            write(expectedKeysQuery);

            write(string.Empty);

            write("Actual keys query:");
            write(groupedQuery.KeysQuery);

            Assert.Equal(expectedKeysQuery, groupedQuery.KeysQuery, StringComparer.Ordinal);
            write(string.Empty);

            write("Expected values query:");
            write(expectedValuesQuery);

            write(string.Empty);

            write("Actual values query:");
            write(groupedQuery.ValuesQuery);

            Assert.Equal(expectedValuesQuery, groupedQuery.ValuesQuery, StringComparer.Ordinal);
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