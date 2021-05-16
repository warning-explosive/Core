namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns;
    using DataAccess.Contract.Abstractions;
    using DataAccess.Orm.Abstractions;
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
                typeof(DataAccess.Orm.PostgreSql.QueryTranslator).Assembly, // DataAccess.Orm.PostgreSql
                typeof(DataAccess.PostgreSql.Settings.PostgreSqlSettings).Assembly, // DataAccess.PostgreSql
                typeof(CrossCuttingConcernsManualRegistration).Assembly, // CrossCuttingConcerns
            };

            yield return new object[]
            {
                "All",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All()),
                $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)}"
            };
            yield return new object[]
            {
                "One property projection - guid",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.Id)),
                $"SELECT{Environment.NewLine}\te.{nameof(TestAggregate.Id)}{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e"
            };
            yield return new object[]
            {
                "One property projection - bool",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.BooleanField)),
                $"SELECT{Environment.NewLine}\te.{nameof(TestAggregate.BooleanField)}{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e"
            };
            yield return new object[]
            {
                "One property projection - string",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.StringField)),
                $"SELECT{Environment.NewLine}\te.{nameof(TestAggregate.StringField)}{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e"
            };
            yield return new object[]
            {
                "One property projection - int",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.IntField)),
                $"SELECT{Environment.NewLine}\te.{nameof(TestAggregate.IntField)}{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e"
            };
            yield return new object[]
            {
                "Boolean property filter",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.BooleanField)),
                $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\te.{nameof(TestAggregate.BooleanField)}"
            };
            yield return new object[]
            {
                "Boolean property filter after anonymous projection",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => new { e.BooleanField, e.StringField }).Where(a => a.BooleanField)),
                $"SELECT{Environment.NewLine}\ta.{nameof(TestAggregate.BooleanField)},{Environment.NewLine}\ta.{nameof(TestAggregate.StringField)}{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} a{Environment.NewLine}WHERE{Environment.NewLine}\ta.{nameof(TestAggregate.BooleanField)}"
            };
            yield return new object[]
            {
                "Property chain",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.StringField.Length)),
                $"SELECT{Environment.NewLine}\te.{nameof(TestAggregate.StringField)}.{nameof(string.Length)}{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e"
            };
            yield return new object[]
            {
                "Binary filter",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.NullableStringField).Where(str => str != null)),
                $"SELECT{Environment.NewLine}\te.{nameof(TestAggregate.NullableStringField)} AS str{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\tstr IS NOT NULL"
            };
            yield return new object[]
            {
                "Ternary projection",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => e.NullableStringField != null ? e.NullableStringField.Length : 0)),
                $"SELECT{Environment.NewLine}\tCASE e.{nameof(TestAggregate.NullableStringField)} IS NOT NULL THEN e.{nameof(TestAggregate.NullableStringField)}.{nameof(string.Length)} ELSE 0{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e"
            };
            yield return new object[]
            {
                "Ternary filter",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.NullableStringField != null ? true : false)),
                $"SELECT{Environment.NewLine}\te.{nameof(TestAggregate.BooleanField)},{Environment.NewLine}\te.{nameof(TestAggregate.Id)},{Environment.NewLine}\te.{nameof(TestAggregate.IntField)},{Environment.NewLine}\te.{nameof(TestAggregate.NullableStringField)},{Environment.NewLine}\te.{nameof(TestAggregate.StringField)},{Environment.NewLine}\te.{nameof(TestAggregate.Version)},{Environment.NewLine}\t(CASE e.{nameof(TestAggregate.NullableStringField)} IS NOT NULL THEN True ELSE False) AS todo_alias_name{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\te.todo_alias_name"
            };
            yield return new object[]
            {
                "Ternary filter after projection",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => new { e.StringField, e.NullableStringField }).Where(p => p.NullableStringField != null ? true : false)),
                $"SELECT{Environment.NewLine}\tp.{nameof(TestAggregate.StringField)},{Environment.NewLine}\tp.{nameof(TestAggregate.NullableStringField)},{Environment.NewLine}\t(CASE p.{nameof(TestAggregate.NullableStringField)} IS NOT NULL THEN True ELSE False) AS todo_alias_name{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} p{Environment.NewLine}WHERE{Environment.NewLine}\tp.todo_alias_name"
            };
            yield return new object[]
            {
                "Ternary filter after projection with renaming",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => new { e.StringField, Filter = e.NullableStringField }).Where(p => p.Filter != null ? true : false)),
                $"SELECT{Environment.NewLine}\tp.{nameof(TestAggregate.StringField)},{Environment.NewLine}\tp.{nameof(TestAggregate.NullableStringField)} AS Filter,{Environment.NewLine}\t(CASE p.{nameof(TestAggregate.NullableStringField)} IS NOT NULL THEN True ELSE False) AS todo_alias_name{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} p{Environment.NewLine}WHERE{Environment.NewLine}\tp.todo_alias_name"
            };
            yield return new object[]
            {
                "Binary comparison ==",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField == 0)),
                $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\te.{nameof(TestAggregate.IntField)} = 0"
            };
            yield return new object[]
            {
                "Binary comparison !=",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField != 0)),
                $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\te.{nameof(TestAggregate.IntField)} != 0"
            };
            yield return new object[]
            {
                "Binary comparison >=",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField >= 0)),
                $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\te.{nameof(TestAggregate.IntField)} >= 0"
            };
            yield return new object[]
            {
                "Binary comparison >",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField > 0)),
                $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\te.{nameof(TestAggregate.IntField)} > 0"
            };
            yield return new object[]
            {
                "Binary comparison <",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField < 0)),
                $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\te.{nameof(TestAggregate.IntField)} < 0"
            };
            yield return new object[]
            {
                "Binary comparison <=",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Where(e => e.IntField <= 0)),
                $"SELECT{Environment.NewLine}\t*{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} e{Environment.NewLine}WHERE{Environment.NewLine}\te.{nameof(TestAggregate.IntField)} <= 0"
            };
            yield return new object[]
            {
                "Change anonymous projection parameter name",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(e => new { e.NullableStringField, e.StringField }).Where(a => a.NullableStringField != null)),
                $"SELECT{Environment.NewLine}\ta.{nameof(TestAggregate.NullableStringField)},{Environment.NewLine}\ta.{nameof(TestAggregate.StringField)}{Environment.NewLine}FROM{Environment.NewLine}\ttodo_database.todo_schema.{nameof(TestAggregate)} a{Environment.NewLine}WHERE{Environment.NewLine}\ta.{nameof(TestAggregate.NullableStringField)} IS NOT NULL"
            };
            yield return new object[]
            {
                "Projection/filter chain",
                nameof(DataAccess.Orm.PostgreSql),
                assemblies,
                new Func<IReadRepository<TestAggregate>, IQueryable>(repository => repository.All().Select(a => new { a.NullableStringField, a.StringField, a.IntField }).Select(b => new { b.NullableStringField, b.IntField }).Where(c => c.NullableStringField != null).Select(d => new { d.IntField }).Where(e => e.IntField > 0).Where(f => f.IntField < 42).Select(g => g.IntField)),
                $"SELECT{Environment.NewLine}\tg.{nameof(TestAggregate.IntField)}{Environment.NewLine}FROM{Environment.NewLine}\t(SELECT{Environment.NewLine}\t\tf.{nameof(TestAggregate.IntField)}{Environment.NewLine}\tFROM{Environment.NewLine}\t\t(SELECT{Environment.NewLine}\t\t\tc.{nameof(TestAggregate.NullableStringField)},{Environment.NewLine}\t\t\tc.{nameof(TestAggregate.IntField)}{Environment.NewLine}\t\tFROM{Environment.NewLine}\t\t\t(SELECT{Environment.NewLine}\t\t\t\ta.{nameof(TestAggregate.NullableStringField)},{Environment.NewLine}\t\t\t\ta.{nameof(TestAggregate.StringField)},{Environment.NewLine}\t\t\t\ta.{nameof(TestAggregate.IntField)}{Environment.NewLine}\t\t\tFROM{Environment.NewLine}\t\t\t\ttodo_database.todo_schema.TestEntity a) c{Environment.NewLine}\t\tWHERE{Environment.NewLine}\t\t\tc.{nameof(TestAggregate.NullableStringField)} IS NOT NULL) f{Environment.NewLine}\tWHERE{Environment.NewLine}\t\tf.{nameof(TestAggregate.IntField)} > 0 AND f.{nameof(TestAggregate.IntField)} < 42) g"
            };
        }

        [Theory]
        [MemberData(nameof(DataAccessTestData))]
        internal void ReadRepositoryQueryAllTest(
            string section,
            string databaseName,
            Assembly[] assemblies,
            Func<IReadRepository<TestAggregate>, IQueryable> queryProducer,
            string expectedQuery)
        {
            Output.WriteLine($"{databaseName}");
            Output.WriteLine($"{section}{Environment.NewLine}");

            var options = new DependencyContainerOptions();

            var dependencyContainer = Fixture.BoundedAboveContainer(options, assemblies);

            using (dependencyContainer.OpenScope())
            {
                var readRepository = dependencyContainer.Resolve<IReadRepository<TestAggregate>>();

                var translatedQuery = dependencyContainer
                    .Resolve<IQueryTranslator>()
                    .Translate(queryProducer(readRepository).Expression);

                Output.WriteLine("Expected query:");
                Output.WriteLine(expectedQuery);
                Output.WriteLine($"{Environment.NewLine}Actual query:");
                Output.WriteLine(translatedQuery.Query);
                Assert.Equal(expectedQuery, translatedQuery.Query);

                /* TODO: Read from real database
                _ = queryProducer(readRepository)
                    .GetEnumerator()
                    .ToObjectEnumerable()
                    .ToList();
                */

                /* TODO: IAsyncQueryable extensions
                _ = await readRepository
                    .All()
                    .Select(entity => entity.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
                */
            }
        }

        internal class TestAggregate : EntityBase, IAggregate
        {
            internal TestAggregate(bool booleanField, string stringField)
            {
                BooleanField = booleanField;
                StringField = stringField;
            }

            public bool BooleanField { get; }

            public string StringField { get; }

            public string? NullableStringField { get; set; }

            public int IntField { get; set; }
        }
    }
}