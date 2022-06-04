namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Extensions;
    using DataAccess.Orm.Host;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.PostgreSql.Host;
    using DataAccess.Orm.Sql.Translation;
    using DataAccess.Orm.Sql.Translation.Extensions;
    using DatabaseEntities;
    using DatabaseEntities.Relations;
    using GenericEndpoint.Host;
    using GenericHost;
    using IntegrationTransport.Host;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Migrations;
    using Mocks;
    using Overrides;
    using Registrations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using SpaceEngineers.Core.DataAccess.Api.Persisting;
    using SpaceEngineers.Core.DataAccess.Api.Reading;
    using SpaceEngineers.Core.DataAccess.Api.Transaction;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DataAccess assemblies test
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class DataAccessTest : TestBase
    {
        /// <summary>
        /// Schema
        /// </summary>
        public const string Schema = "SpaceEngineersCoreGenericHostTest";

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
            var timeout = TimeSpan.FromSeconds(60);

            var postgreSqlDatabaseProvider = new PostgreSqlDatabaseProvider();

            var useInMemoryIntegrationTransport = new Func<string, ITestOutputHelper, IHostBuilder, IHostBuilder>(
                (test, output, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithOverrides(new TestLoggerOverride(output))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var emptyQueryParameters = new Dictionary<string, object?>();

            var testDatabaseEntity = new DatabaseEntity(Guid.NewGuid(), true, "SomeString", "SomeNullableString", 42);
            var user = new User(Guid.NewGuid(), "SpaceEngineer");
            var posts = new List<Post>();
            var blog = new Blog(Guid.NewGuid(), "MilkyWay", posts);
            var post = new Post(Guid.NewGuid(), blog, user, DateTime.Now, "PostContent");
            posts.Add(post);

            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - all",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - anonymous projections chain",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.StringField, it.IntField, it.BooleanField }).Select(it => new { it.StringField, it.IntField }).Select(it => new { it.IntField }).Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a) b) c) d",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison !=",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField != 43)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" != @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 43 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField < 43)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" < @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 43 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <=",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField <= 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" <= @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison ==",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField == 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" = @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField > 41)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" > @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 41 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >=",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField >= 42)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary filter",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.NullableStringField).Where(it => it != null)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter after anonymous projection",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.BooleanField, it.StringField }).Where(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - change anonymous projection parameter name",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.NullableStringField, it.StringField }).Where(it => it.NullableStringField != null)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - coalesce projection",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.NullableStringField ?? string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}COALESCE(a.""{nameof(DatabaseEntity.NullableStringField)}"", @param_0){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        new Dictionary<string, object?> { ["param_0"] = string.Empty },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection to anonymous type",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.BooleanField).Select(it => new { it.StringField }).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection to primitive type",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.BooleanField).Select(it => it.StringField).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with join expression",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post, Guid>>().All().Where(it => it.Blog.Theme == "MilkyWay").Select(it => it.User.Nickname).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}d.""{nameof(User.Nickname)}"" AS ""User_Nickname""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(User)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Blog.PrimaryKey)}"" AS ""Blog_PrimaryKey"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""User_PrimaryKey""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(Blog)}"" b{Environment.NewLine}{'\t'}JOIN{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}ON{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Blog.PrimaryKey)}"" = a.""Blog_PrimaryKey""{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Blog.Theme)}"" = @param_0) c{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(User.PrimaryKey)}"" = c.""User_PrimaryKey""",
                        new Dictionary<string, object?> { ["param_0"] = "MilkyWay" },
                        write)),
                new IUniqueIdentified[] { user, blog, post },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with predicate",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.StringField, it.BooleanField }).Where(it => it.BooleanField).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - anonymous class key test",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().GroupBy(it => new { it.StringField, it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - projection source test",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField >= 42).Select(it => new { it.StringField, it.BooleanField }).GroupBy(it => new { it.StringField, it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0) b) c",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - single field key test",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().GroupBy(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - anonymous class key test",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().GroupBy(it => new { it.StringField, it.BooleanField }, it => new { it.IntField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - projection source test",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.IntField >= 42).Select(it => new { it.StringField, it.BooleanField, it.IntField }).GroupBy(it => new { it.StringField, it.BooleanField }, it => new { it.IntField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0) b) c",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - single field key test",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().GroupBy(it => it.StringField, it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - bool",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - guid",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - int",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - string",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in filter",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post, Guid>>().All().Where(it => it.Blog.Theme == "MilkyWay" && it.User.Nickname == "SpaceEngineer")),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.PrimaryKey)}"" AS ""Blog_PrimaryKey"",{Environment.NewLine}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}b.""{nameof(Post.User.PrimaryKey)}"" AS ""User_PrimaryKey""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""PrimaryKey"" = a.""User_PrimaryKey""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""PrimaryKey"" = a.""Blog_PrimaryKey""{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}c.""{nameof(Blog.Theme)}"" = @param_0 AND b.""{nameof(User.Nickname)}"" = @param_1",
                        new Dictionary<string, object?> { ["param_0"] = "MilkyWay", ["param_1"] = "SpaceEngineer" },
                        write)),
                new IUniqueIdentified[] { user, blog, post },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection with filter as source",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post, Guid>>().All().Where(it => it.DateTime > DateTime.MinValue).Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(Blog.Theme)}"" AS ""Blog_Theme"",{Environment.NewLine}{'\t'}c.""{nameof(User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Blog)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(User)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""Blog_PrimaryKey"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""User_PrimaryKey""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"" > @param_0) b{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(User.PrimaryKey)}"" = b.""User_PrimaryKey""{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(Blog.PrimaryKey)}"" = b.""Blog_PrimaryKey""",
                        new Dictionary<string, object?> { ["param_0"] = DateTime.MinValue },
                        write)),
                new IUniqueIdentified[] { user, blog, post },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post, Guid>>().All().Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Blog.Theme)}"" AS ""Blog_Theme"",{Environment.NewLine}{'\t'}b.""{nameof(User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""PrimaryKey"" = a.""User_PrimaryKey""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""PrimaryKey"" = a.""Blog_PrimaryKey""",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { user, blog, post },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - projection/filter chain",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.NullableStringField, it.StringField, it.IntField }).Select(it => new { it.NullableStringField, it.IntField }).Where(it => it.NullableStringField != null).Select(it => new { it.IntField }).Where(it => it.IntField > 0).Where(it => it.IntField <= 42).Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a) b{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL) c{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}"" > @param_0 AND c.""{nameof(DatabaseEntity.IntField)}"" <= @param_1) d",
                        new Dictionary<string, object?> { ["param_0"] = 0, ["param_1"] = 42 },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - property chain with translated member",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.StringField.Length)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}length(a.""{nameof(DatabaseEntity.StringField)}""){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - all",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().All(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(CASE WHEN a.""{nameof(DatabaseEntity.BooleanField)}"" THEN 1 ELSE NULL END) = Count(*)) AS ""All""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - any",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Any(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*) > 0) AS ""Any""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - count",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Count(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*)) AS ""Count""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().First(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}fetch first 1 rows only",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single async by primary key",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().SingleAsync(testDatabaseEntity.PrimaryKey, CancellationToken.None)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = testDatabaseEntity.PrimaryKey },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single by primary key",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().Single(testDatabaseEntity.PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = testDatabaseEntity.PrimaryKey },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default async by primary key",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().SingleOrDefaultAsync(testDatabaseEntity.PrimaryKey, CancellationToken.None)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = testDatabaseEntity.PrimaryKey },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default by primary key",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().SingleOrDefault(testDatabaseEntity.PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = testDatabaseEntity.PrimaryKey },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Scalar result - single",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Single(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}fetch first 2 rows only",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sub-query",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container =>
                {
                    var subQuery = container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.PrimaryKey);
                    return container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => subQuery.Contains(it.PrimaryKey));
                }),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" IN (SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a)",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection with renaming",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.StringField, Filter = it.NullableStringField }).Where(it => it.Filter != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" AS ""Filter""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { it.StringField, it.NullableStringField }).Where(it => it.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => it.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary projection",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => it.NullableStringField != null ? it.NullableStringField : string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" ELSE @param_0 END{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        new Dictionary<string, object?> { ["param_0"] = string.Empty },
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary filter",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Where(it => !it.BooleanField || it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}"" OR a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection to anonymous class",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => new { Negation = !it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(NOT a.""{nameof(DatabaseEntity.BooleanField)}"") AS ""Negation""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection",
                useInMemoryIntegrationTransport,
                postgreSqlDatabaseProvider,
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity, Guid>>().All().Select(it => !it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, write) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{Schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        write)),
                new IUniqueIdentified[] { testDatabaseEntity },
                timeout
            };
        }

        [Fact]
        internal void NextLambdaParameterNameTest()
        {
            var ctx = new TranslationContext();

            var producers = Enumerable
                .Range(0, 1000)
                .Select(_ => ctx.NextLambdaParameterName())
                .Reverse()
                .ToArray();

            ctx.ReverseLambdaParametersNames();

            Assert.Equal("a", producers[0]());
            Assert.Equal("b", producers[1]());
            Assert.Equal("c", producers[2]());
            Assert.Equal("d", producers[3]());

            Assert.Equal("y", producers[24]());
            Assert.Equal("z", producers[25]());
            Assert.Equal("aa", producers[26]());
            Assert.Equal("ab", producers[27]());

            Assert.Equal("zy", producers[700]());
            Assert.Equal("zz", producers[701]());
            Assert.Equal("aaa", producers[702]());
            Assert.Equal("aab", producers[703]());
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(QueryTranslationTestData))]
        internal async Task QueryTranslationTest(
            string section,
            Func<string, ITestOutputHelper, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider,
            Func<IDependencyContainer, object?> queryProducer,
            Action<IQuery, Action<string>> checkQuery,
            IUniqueIdentified[] databaseEntities,
            TimeSpan timeout)
        {
            Output.WriteLine(section);

            var additionalOurTypes = new[]
            {
                typeof(DatabaseEntity),
                typeof(Blog),
                typeof(Post),
                typeof(User),
                typeof(Community),
                typeof(Participant)
            };

            var manualMigrations = new Type[]
            {
                typeof(CreateOrGetExistedPostgreSqlDatabaseManualMigration)
            };

            var manualRegistrations = new IManualRegistration[]
            {
                new QueryExpressionsCollectorManualRegistration()
            };

            var settingsScope = nameof(QueryTranslationTest);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(Output),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(nameof(QueryTranslationTest), Output, Host.CreateDefaultBuilder())
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithManualRegistrations(manualRegistrations)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(manualMigrations)
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            var dependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await dependencyContainer
                   .InvokeWithinTransaction(
                        false,
                        Run(dependencyContainer, queryProducer, checkQuery, databaseEntities, Output.WriteLine),
                        cts.Token)
                   .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        private static Func<IDatabaseTransaction, CancellationToken, Task> Run(
            IDependencyContainer dependencyContainer,
            Func<IDependencyContainer, object?> queryProducer,
            Action<IQuery, Action<string>> checkQuery,
            IUniqueIdentified[] databaseEntities,
            Action<string> log)
        {
            return async (_, token) =>
            {
                var expression = (queryProducer(dependencyContainer) as IQueryable)?.Expression
                              ?? dependencyContainer.Resolve<QueryExpressionsCollector>().Expressions.Single();

                var query = dependencyContainer
                   .Resolve<IQueryProvider>()
                   .CreateQuery(expression);

                var translatedQuery = dependencyContainer
                   .Resolve<IQueryTranslator>()
                   .Translate(expression);

                checkQuery(translatedQuery, log);

                await Insert(dependencyContainer, databaseEntities, token)
                   .ConfigureAwait(false);

                var queryResult = query
                   .GetEnumerator()
                   .AsEnumerable<object>()
                   .ToList();

                Assert.Single(queryResult);
                var item = queryResult.Single();
                var dump = item.GetType().IsPrimitive()
                    ? item.ToString() !
                    : item.ShowProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                log(dump);

                foreach (var @object in queryResult)
                {
                    if (@object.GetType().IsSubclassOfOpenGeneric(typeof(IGrouping<,>))
                        && translatedQuery is GroupedQuery groupedQuery)
                    {
                        var keyValues = @object
                            .GetPropertyValue(nameof(IGrouping<object, object>.Key))
                            .AsQueryParametersValues();

                        log("Actual key values:");
                        log(keyValues.Select(pair => pair.ToString()).ToString(Environment.NewLine));

                        var valuesExpression = groupedQuery.ValuesExpressionProducer.Invoke(keyValues);
                        var valuesQuery = valuesExpression.Translate(dependencyContainer, 0);
                        var valuesQueryParameters = valuesExpression.ExtractQueryParameters();

                        log("Actual values query parameters:");
                        log(valuesQueryParameters.Select(pair => pair.ToString()).ToString(Environment.NewLine));

                        log("Actual values query:");
                        log(valuesQuery);

                        log("Actual values:");
                        var enumerator = ((IEnumerable)@object).GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            log(enumerator.Current.ToString() !);
                        }
                    }
                    else if (translatedQuery is FlatQuery)
                    {
                        log("Flat query has no additional verifications");
                    }
                    else
                    {
                        throw new NotSupportedException(translatedQuery.ToString());
                    }
                }
            };
        }

        private static Task Insert(
            IDependencyContainer dependencyContainer,
            IUniqueIdentified[] entities,
            CancellationToken token)
        {
            return dependencyContainer
               .Resolve<IRepository>()
               .Insert(entities, EnInsertBehavior.Default, token);
        }

        private static void CheckFlatQuery(
            IQuery query,
            string expectedQuery,
            IReadOnlyDictionary<string, object?> expectedQueryParameters,
            Action<string> log)
        {
            var flatQuery = (FlatQuery)query;

            log("Expected query:");
            log(expectedQuery);

            log("Actual query:");
            log(flatQuery.Query);

            Assert.Equal(expectedQuery, flatQuery.Query, StringComparer.Ordinal);
            CheckParameters(flatQuery.QueryParameters, expectedQueryParameters);
        }

        private static void CheckGroupedQuery(
            IQuery query,
            string expectedKeysQuery,
            IReadOnlyDictionary<string, object?> expectedKeysQueryParameters,
            Action<string> log)
        {
            var groupedQuery = (GroupedQuery)query;

            log("Expected keys query:");
            log(expectedKeysQuery);

            log("Actual keys query:");
            log(groupedQuery.KeysQuery);

            Assert.Equal(expectedKeysQuery, groupedQuery.KeysQuery, StringComparer.Ordinal);
            CheckParameters(groupedQuery.KeysQueryParameters, expectedKeysQueryParameters);
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