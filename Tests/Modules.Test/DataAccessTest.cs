namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Registration;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.Api.Reading;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Sql.Translation;
    using DatabaseEntities;
    using DatabaseEntities.Relations;
    using GenericHost.Internals;
    using Mocks;
    using Registrations;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DataAccess assemblies test
    /// </summary>
    public class DataAccessTest : TestBase
    {
        /// <summary>
        /// Schema
        /// </summary>
        public const string Schema = "SpaceEngineersCoreModulesTest";

        /// <summary>
        /// Primary key
        /// </summary>
        public static readonly Guid PrimaryKey = Guid.NewGuid();

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DataAccessTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// QueryTranslationTestData
        /// </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object> QueryTranslationTestData()
        {
            var emptyQueryParameters = new Dictionary<string, object?>();

            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - all",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison !=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField != 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" != @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField < 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" < @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField <= 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" <= @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison ==",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField == 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" = @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField > 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" > @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField >= 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.NullableStringField).Where(it => it != null)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter after anonymous projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.BooleanField, it.StringField }).Where(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - change anonymous projection parameter name",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.NullableStringField, it.StringField }).Where(it => it.NullableStringField != null)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - coalesce projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.NullableStringField ?? string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}COALESCE(a.""{nameof(DatabaseEntity.NullableStringField)}"", @param_0){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        new Dictionary<string, object?> { ["param_0"] = string.Empty },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with predicate",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => !it.BooleanField).Select(outbox => outbox.StringField).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - anonymous class key test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().GroupBy(it => new { it.StringField, it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - projection source test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField >= 42).Select(it => new { it.StringField, it.BooleanField }).GroupBy(it => new { it.StringField, it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0) b",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - single field key test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().GroupBy(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - anonymous class key test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().GroupBy(it => new { it.StringField, it.BooleanField }, it => new { it.IntField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - projection source test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField >= 42).Select(it => new { it.StringField, it.BooleanField, it.IntField }).GroupBy(it => new { it.StringField, it.BooleanField }, it => new { it.IntField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0) b) c",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - single field key test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().GroupBy(it => it.StringField, it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - bool",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - guid",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - int",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - string",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post, Guid>>().All().Where(it => it.Blog.Theme == "qwerty" && it.User.Nickname == "SpaceEngineer")),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.PrimaryKey)}"" AS ""Blog_PrimaryKey"",{Environment.NewLine}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}b.""{nameof(Post.User.PrimaryKey)}"" AS ""User_PrimaryKey""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""PrimaryKey"" = a.""User_PrimaryKey""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""PrimaryKey"" = a.""Blog_PrimaryKey""{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}c.""{nameof(Blog.Theme)}"" = @param_0 AND b.""{nameof(User.Nickname)}"" = @param_1",
                        new Dictionary<string, object?> { ["param_0"] = "qwerty", ["param_1"] = "SpaceEngineer" },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection with filter as source",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post, Guid>>().All().Where(it => it.DateTime > DateTime.MinValue).Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(Blog.Theme)}"" AS ""Blog_Theme"",{Environment.NewLine}{'\t'}(c.""{nameof(User.Nickname)}"" AS ""User_Nickname"") AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Blog)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(User)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""Blog_PrimaryKey"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""User_PrimaryKey""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"" > @param_0) b{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(User.PrimaryKey)}"" = b.""User_PrimaryKey""{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(Blog.PrimaryKey)}"" = b.""Blog_PrimaryKey""",
                        new Dictionary<string, object?> { ["param_0"] = DateTime.MinValue },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post, Guid>>().All().Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Blog.Theme)}"" AS ""Blog_Theme"",{Environment.NewLine}{'\t'}(b.""{nameof(User.Nickname)}"" AS ""User_Nickname"") AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""PrimaryKey"" = a.""User_PrimaryKey""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""PrimaryKey"" = a.""Blog_PrimaryKey""",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - projection/filter chain",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.NullableStringField, it.StringField, it.IntField }).Select(it => new { it.NullableStringField, it.IntField }).Where(it => it.NullableStringField != null).Select(it => new { it.IntField }).Where(it => it.IntField > 0).Where(it => it.IntField < 42).Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a) b{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL) c{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}"" > @param_0 AND c.""{nameof(DatabaseEntity.IntField)}"" < @param_1) d",
                        new Dictionary<string, object?> { ["param_0"] = 0, ["param_1"] = 42 },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - property chain with translated member",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.StringField.Length)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}length(a.""{nameof(DatabaseEntity.StringField)}""){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - all",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().All(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(CASE WHEN a.""{nameof(DatabaseEntity.BooleanField)}"" THEN 1 ELSE NULL END) = Count(*)) AS ""All""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - any",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Any(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*) > 0) AS ""Any""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - count",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Count(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*)) AS ""Count""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().First(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}fetch first 1 rows only",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single async by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().SingleAsync(PrimaryKey, CancellationToken.None)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = PrimaryKey },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().Single(PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = PrimaryKey },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default async by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().SingleOrDefaultAsync(PrimaryKey, CancellationToken.None)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = PrimaryKey },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().SingleOrDefault(PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = PrimaryKey },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Scalar result - single",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Single(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}fetch first 2 rows only",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sub-query",
                new Func<IDependencyContainer, object?>(container =>
                {
                    var subQuery = container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.PrimaryKey);
                    return container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => subQuery.Contains(it.PrimaryKey));
                }),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" IN (SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a)",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection with renaming",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.StringField, Filter = it.NullableStringField }).Where(it => it.Filter != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" AS ""Filter""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.StringField, it.NullableStringField }).Where(it => it.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.NullableStringField != null ? it.NullableStringField : string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" ELSE @param_0 END{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        new Dictionary<string, object?> { ["param_0"] = string.Empty },
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => !it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection to anonymous class",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { Negation = !it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(NOT a.""{nameof(DatabaseEntity.BooleanField)}"") AS ""Negation""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => !it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write))
            };
        }

        [Fact]
        internal void NextLambdaParameterNameTest()
        {
            var ctx = new TranslationContext();

            var producers = Enumerable
                .Range(0, 42)
                .Select(_ => ctx.NextLambdaParameterName())
                .Reverse()
                .ToArray();

            ctx.ReverseLambdaParametersNames();

            foreach (var producer in producers)
            {
                Output.WriteLine(producer());
            }

            Assert.Equal("a", producers[0]());
            Assert.Equal("b", producers[1]());
            Assert.Equal("c", producers[2]());
            Assert.Equal("y", producers[24]());
            Assert.Equal("z", producers[25]());
            Assert.Equal("aa", producers[26]());
            Assert.Equal("ab", producers[27]());
            Assert.Equal("ao", producers[40]());
            Assert.Equal("ap", producers[41]());
        }

        [Theory]
        [MemberData(nameof(QueryTranslationTestData))]
        internal void QueryTranslationTest(
            string section,
            Func<IDependencyContainer, object?> queryProducer,
            Action<IQuery, Action<string>> checkQuery)
        {
            Output.WriteLine(section);
            Output.WriteLine(string.Empty);

            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Dynamic)))
            };

            var additionalOurTypes = new[]
            {
                typeof(Blog),
                typeof(Post),
                typeof(User),
                typeof(Community),
                typeof(Participant),
            };

            var manualRegistrations = new IManualRegistration[]
            {
                new QueryExpressionsCollectorManualRegistration(),
                new ConfigurationProviderManualRegistration()
            };

            var options = new DependencyContainerOptions()
                .WithAdditionalOurTypes(additionalOurTypes)
                .WithManualRegistrations(manualRegistrations);

            var dependencyContainer = Fixture.BoundedAboveContainer(Output, options, assemblies);

            using (dependencyContainer.OpenScope())
            {
                var query = ExecutionExtensions
                    .Try(dependencyContainer, queryProducer)
                    .Catch<Exception>()
                    .Invoke(ex => ex);

                var queryTranslator = dependencyContainer.Resolve<IQueryTranslator>();

                var expression = query is IQueryable queryable
                    ? queryable.Expression
                    : dependencyContainer
                        .Resolve<QueryExpressionsCollector>()
                        .Expressions
                        .Single();

                var translatedQuery = queryTranslator.Translate(expression);

                checkQuery(translatedQuery, Output.WriteLine);

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
                        var valuesQuery = valuesExpression.Translate(DependencyContainer, 0, token).Result;
                        var valuesQueryParameters = valuesExpression
                            .ExtractQueryParameters(DependencyContainer)
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

        private static void CheckParameters(
            IReadOnlyDictionary<string, object?> actualQueryParameters,
            IReadOnlyDictionary<string, object?> expectedQueryParameters)
        {
            var parameters = actualQueryParameters
                .FullOuterJoin(expectedQueryParameters,
                    actual => actual.Key,
                    expected => expected.Key,
                    (actual, expected) => (actual, expected),
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var (actual, expected) in parameters)
            {
                Assert.Equal(expected.Value, actual.Value);
            }
        }
    }
}