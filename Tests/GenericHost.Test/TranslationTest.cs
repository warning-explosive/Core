namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Extensions;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Translation;
    using DataAccess.Orm.Sql.Translation.Extensions;
    using DatabaseEntities;
    using DatabaseEntities.Relations;
    using GenericEndpoint.DataAccess.Host;
    using GenericEndpoint.Host;
    using GenericHost;
    using IntegrationTransport.Host;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Migrations;
    using Mocks;
    using Registrations;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using SpaceEngineers.Core.DataAccess.Api.Persisting;
    using SpaceEngineers.Core.DataAccess.Api.Reading;
    using SpaceEngineers.Core.DataAccess.Api.Transaction;
    using SpaceEngineers.Core.Test.Api;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// TranslationTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class TranslationTest : TestBase
    {
        private static ITestOutputHelper? _staticOutput;
        private static TestFixture? _staticFixture;

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public TranslationTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            _staticOutput = output;
            _staticFixture = fixture;
        }

        private static ITestOutputHelper StaticOutput => _staticOutput.EnsureNotNull(nameof(_staticOutput));

        private static TestFixture StaticFixture => _staticFixture.EnsureNotNull(nameof(_staticFixture));

        /// <summary>
        /// QueryTranslationTestData
        /// </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> QueryTranslationTestData()
        {
            var hosts = QueryTranslationTestHosts().ToArray();
            var testCases = QueryTranslationTestCases().ToArray();
            var countdownEvent = new AsyncCountdownEvent(testCases.Length);

            return hosts
               .SelectMany(host => testCases
                   .Select(testCase => host
                       .Concat(new object[] { countdownEvent })
                       .Concat(testCase)
                       .ToArray()));
        }

        internal static IEnumerable<object[]> QueryTranslationTestHosts()
        {
            var settingsDirectory = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory wasn't found")
               .StepInto("Settings")
               .StepInto(nameof(QueryTranslationTest));

            var timeout = TimeSpan.FromSeconds(60);

            var cts = new CancellationTokenSource(timeout);

            var host = new Lazy<IHost>(() =>
                {
                    var hostBuilder = StaticFixture.CreateHostBuilder(StaticOutput);

                    var databaseEntities = new[]
                    {
                        typeof(DatabaseEntity),
                        typeof(Blog),
                        typeof(Post),
                        typeof(User),
                        typeof(Community),
                        typeof(Participant)
                    };

                    var manualMigrations = new[]
                    {
                        typeof(CreateOrGetExistedPostgreSqlDatabaseMigration)
                    };

                    var additionalOurTypes = databaseEntities
                       .Concat(manualMigrations)
                       .ToArray();

                    var manualRegistrations = new IManualRegistration[]
                    {
                        new QueryExpressionsCollectorManualRegistration(),
                        new IsolationLevelManualRegistration(IsolationLevel.ReadCommitted)
                    };

                    var host = hostBuilder
                       .UseIntegrationTransport(builder => builder
                           .WithInMemoryIntegrationTransport(hostBuilder)
                           .BuildOptions())
                       .UseEndpoint(TestIdentity.Endpoint10,
                            (_, builder) => builder
                               .WithPostgreSqlDataAccess(options => options
                                   .ExecuteMigrations())
                               .ModifyContainerOptions(options => options
                                   .WithAdditionalOurTypes(additionalOurTypes)
                                   .WithManualRegistrations(manualRegistrations))
                               .BuildOptions())
                       .BuildHost(settingsDirectory);

                    var awaiter = host.WaitUntilTransportIsNotRunning(cts.Token);

                    host.StartAsync(cts.Token).Wait(cts.Token);

                    awaiter.Wait(cts.Token);

                    return host;
                },
                LazyThreadSafetyMode.ExecutionAndPublication);

            yield return new object[] { host, cts };
        }

        internal static IEnumerable<object[]> QueryTranslationTestCases()
        {
            var emptyQueryParameters = new Dictionary<string, object?>();

            var schema = nameof(GenericHost) + nameof(Test);
            var testDatabaseEntity = DatabaseEntity.Generate();
            var user = new User(Guid.NewGuid(), "SpaceEngineer");
            var posts = new List<Post>();
            var blog = new Blog(Guid.NewGuid(), "MilkyWay", posts);
            var post = new Post(Guid.NewGuid(), blog, user, DateTime.Now, "PostContent");
            posts.Add(post);

            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - all",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All()),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - anonymous projections chain",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => new { it.StringField, it.IntField, it.BooleanField }).Select(it => new { it.StringField, it.IntField }).Select(it => new { it.IntField }).Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a) b) c) d",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison !=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.IntField != 43)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" != @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 43 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.IntField < 43)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" < @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 43 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.IntField <= 42)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" <= @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison ==",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.IntField == 42)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" = @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.IntField > 41)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" > @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 41 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.IntField >= 42)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.NullableStringField).Where(it => it != null)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter after anonymous projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => new { it.BooleanField, it.StringField }).Where(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - change anonymous projection parameter name",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => new { it.NullableStringField, it.StringField }).Where(it => it.NullableStringField != null)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - coalesce projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.NullableStringField ?? string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}COALESCE(a.""{nameof(DatabaseEntity.NullableStringField)}"", @param_0){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new Dictionary<string, object?> { ["param_0"] = string.Empty },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection to anonymous type",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.BooleanField).Select(it => new { it.StringField }).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection to primitive type",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.BooleanField).Select(it => it.StringField).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with join expression",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post>>().All().Where(it => it.Blog.Theme == "MilkyWay").Select(it => it.User.Nickname).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}d.""{nameof(Post.User.Nickname)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.Nickname)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Blog)}"" b{Environment.NewLine}{'\t'}JOIN{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}ON{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Blog.Theme)}"" = @param_0) c{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(User.PrimaryKey)}"" = c.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""",
                        new Dictionary<string, object?> { ["param_0"] = "MilkyWay" },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with predicate",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => new { it.StringField, it.BooleanField }).Where(it => it.BooleanField).Distinct()),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - anonymous class key test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().GroupBy(it => new { it.StringField, it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - projection source test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.IntField >= 42).Select(it => new { it.StringField, it.BooleanField }).GroupBy(it => new { it.StringField, it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0) b) c",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`2 - single field key test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().GroupBy(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - anonymous class key test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().GroupBy(it => new { it.StringField, it.BooleanField }, it => new { it.IntField })),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - projection source test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.IntField >= 42).Select(it => new { it.StringField, it.BooleanField, it.IntField }).GroupBy(it => new { it.StringField, it.BooleanField }, it => new { it.IntField })),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0) b) c",
                        new Dictionary<string, object?> { ["param_0"] = 42 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - groupBy`3 - single field key test",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().GroupBy(it => it.StringField, it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckGroupedQuery(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - bool",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - guid",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - int",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - string",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post>>().All().Where(it => it.Blog.Theme == "MilkyWay" && it.User.Nickname == "SpaceEngineer")),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}b.""{nameof(Post.User.PrimaryKey)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(User.PrimaryKey)}"" = a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}c.""{nameof(Blog.Theme)}"" = @param_0 AND b.""{nameof(User.Nickname)}"" = @param_1",
                        new Dictionary<string, object?> { ["param_0"] = "MilkyWay", ["param_1"] = "SpaceEngineer" },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection with filter as source",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post>>().All().Where(it => it.DateTime > DateTime.MinValue).Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(Post.Blog.Theme)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.Theme)}"",{Environment.NewLine}{'\t'}c.""{nameof(User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"" > @param_0) b{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(User.PrimaryKey)}"" = b.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(Blog.PrimaryKey)}"" = b.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""",
                        new Dictionary<string, object?> { ["param_0"] = DateTime.MinValue },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post>>().All().Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.Theme)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.Theme)}"",{Environment.NewLine}{'\t'}b.""{nameof(User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(User.PrimaryKey)}"" = a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by join",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<Post>>().All().OrderByDescending(it => it.Blog.Theme).ThenBy(it => it.User.Nickname)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}b.""{nameof(Post.User.PrimaryKey)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(User.PrimaryKey)}"" = a.""{nameof(post.User)}_{nameof(post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(post.Blog)}_{nameof(post.Blog.PrimaryKey)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}c.""{nameof(Blog.Theme)}"" DESC, b.""{nameof(User.Nickname)}"" ASC",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by then by",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.BooleanField).OrderBy(it => it.IntField).ThenByDescending(it => it.StringField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" ASC, a.""{nameof(DatabaseEntity.StringField)}"" DESC",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.BooleanField).OrderBy(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" ASC",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - projection/filter chain",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => new { it.NullableStringField, it.StringField, it.IntField }).Select(it => new { it.NullableStringField, it.IntField }).Where(it => it.NullableStringField != null).Select(it => new { it.IntField }).Where(it => it.IntField > 0).Where(it => it.IntField <= 42).Select(it => it.IntField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a) b{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL) c{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}"" > @param_0 AND c.""{nameof(DatabaseEntity.IntField)}"" <= @param_1) d",
                        new Dictionary<string, object?> { ["param_0"] = 0, ["param_1"] = 42 },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - property chain with translated member",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.StringField.Length)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}length(a.""{nameof(DatabaseEntity.StringField)}""){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - all",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().All(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(CASE WHEN a.""{nameof(DatabaseEntity.BooleanField)}"" THEN 1 ELSE NULL END) = Count(*)) AS ""All""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - any",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Any(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*) > 0) AS ""Any""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - count",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Count(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*)) AS ""Count""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().First(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}fetch first 1 rows only",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single async by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().SingleAsync(testDatabaseEntity.PrimaryKey, CancellationToken.None)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = testDatabaseEntity.PrimaryKey },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().Single(testDatabaseEntity.PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = testDatabaseEntity.PrimaryKey },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default async by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().SingleOrDefaultAsync(testDatabaseEntity.PrimaryKey, CancellationToken.None)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = testDatabaseEntity.PrimaryKey },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().SingleOrDefault(testDatabaseEntity.PrimaryKey)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new Dictionary<string, object?> { ["param_0"] = testDatabaseEntity.PrimaryKey },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - Scalar result - single",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Single(it => it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}fetch first 2 rows only",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sub-query",
                new Func<IDependencyContainer, object?>(container =>
                {
                    var subQuery = container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.PrimaryKey);
                    return container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => subQuery.Contains(it.PrimaryKey));
                }),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" IN (SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a)",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection with renaming",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => new { it.StringField, Filter = it.NullableStringField }).Where(it => it.Filter != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" AS ""Filter""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => new { it.StringField, it.NullableStringField }).Where(it => it.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => it.NullableStringField != null ? true : false)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN @param_0 ELSE @param_1 END",
                        new Dictionary<string, object?> { ["param_0"] = true, ["param_1"] = false },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => it.NullableStringField != null ? it.NullableStringField : string.Empty)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}CASE WHEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" ELSE @param_0 END{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new Dictionary<string, object?> { ["param_0"] = string.Empty },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Where(it => !it.BooleanField || it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}"" OR a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection to anonymous class",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => new { Negation = !it.BooleanField })),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(NOT a.""{nameof(DatabaseEntity.BooleanField)}"") AS ""Negation""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseEntity>>().All().Select(it => !it.BooleanField)),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sql view tanslation",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository<DatabaseColumn>>().All().Select(it => new { it.Schema, it.Table, it.Column }).First()),
                new Action<IQuery, Action<string>>(
                    (query, log) => CheckFlatQuery(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Schema)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Table)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Column)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{nameof(DataAccess.Orm.Sql.Host.Migrations)}"".""{nameof(DatabaseColumn)}"" a{Environment.NewLine}fetch first 1 rows only",
                        emptyQueryParameters,
                        log)),
                Array.Empty<IDatabaseEntity>()
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
            Lazy<IHost> host,
            CancellationTokenSource cts,
            AsyncCountdownEvent asyncCountdownEvent,
            string section,
            Func<IDependencyContainer, object?> queryProducer,
            Action<IQuery, Action<string>> checkQuery,
            IDatabaseEntity[] databaseEntities)
        {
            try
            {
                Output.WriteLine(section);

                var endpointDependencyContainer = host.Value.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                var sqlDatabaseSettings = await endpointDependencyContainer
                   .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(nameof(QueryTranslationTest), sqlDatabaseSettings.Database);
                Assert.Equal(IsolationLevel.ReadCommitted, sqlDatabaseSettings.IsolationLevel);
                Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                var hostShutdown = host.Value.WaitForShutdownAsync(cts.Token);

                var assert = endpointDependencyContainer
                   .InvokeWithinTransaction(
                        false,
                        Run(endpointDependencyContainer, queryProducer, checkQuery, databaseEntities, Output.WriteLine),
                        cts.Token);

                var awaiter = Task.WhenAny(hostShutdown, assert);

                var result = await awaiter.ConfigureAwait(false);

                if (hostShutdown == result)
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                await result.ConfigureAwait(false);
            }
            finally
            {
                asyncCountdownEvent.Decrement();

                if (asyncCountdownEvent.Read() == 0)
                {
                    Output.WriteLine("CLEANUP");

                    try
                    {
                        await host
                           .Value
                           .StopAsync(cts.Token)
                           .ConfigureAwait(false);
                    }
                    finally
                    {
                        cts.Dispose();
                        host.Value.Dispose();
                    }
                }
            }
        }

        private static Func<IDatabaseTransaction, CancellationToken, Task> Run(
            IDependencyContainer dependencyContainer,
            Func<IDependencyContainer, object?> queryProducer,
            Action<IQuery, Action<string>> checkQuery,
            IDatabaseEntity[] databaseEntities,
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
            IDatabaseEntity[] entities,
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