namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
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
    using DataAccess.Orm.Settings;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Translation;
    using DataAccess.Orm.Transaction;
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
    using SpaceEngineers.Core.Test.Api;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// TranslationTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    [SuppressMessage("Analysis", "SA1131", Justification = "test case")]
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
        /// CommandTranslationTestData
        /// </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> CommandTranslationTestData()
        {
            var hosts = CommandTranslationTestHosts().ToArray();
            var testCases = CommandTranslationTestCases().ToArray();
            var countdownEvent = new AsyncCountdownEvent(testCases.Length);

            return hosts
               .SelectMany(host => testCases
                   .Select(testCase => host
                       .Concat(new object[] { countdownEvent })
                       .Concat(testCase)
                       .ToArray()));
        }

        internal static IEnumerable<object[]> CommandTranslationTestHosts()
        {
            var settingsDirectory = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory wasn't found")
               .StepInto("Settings")
               .StepInto(nameof(CommandTranslationTest));

            var timeout = TimeSpan.FromSeconds(60);

            var cts = new CancellationTokenSource(timeout);

            var host = new Lazy<IHost>(() =>
                {
                    var hostBuilder = StaticFixture.CreateHostBuilder(StaticOutput);

                    var entities = new[]
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

                    var additionalOurTypes = entities
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

        internal static IEnumerable<object[]> CommandTranslationTestCases()
        {
            var emptyQueryParameters = Array.Empty<SqlCommandParameter>();

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
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>()),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - anonymous projections chain",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => new { it.StringField, it.IntField, it.BooleanField }).Select(it => new { it.StringField, it.IntField }).Select(it => new { it.IntField }).Select(it => it.IntField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a) b) c) d",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison !=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.IntField != 43)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" != @param_0",
                        new[] { new SqlCommandParameter("param_0", 43, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.IntField < 43)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" < @param_0",
                        new[] { new SqlCommandParameter("param_0", 43, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.IntField <= 42)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" <= @param_0",
                        new[] { new SqlCommandParameter("param_0", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison ==",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.IntField == 42)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" = @param_0",
                        new[] { new SqlCommandParameter("param_0", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.IntField > 41)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" > @param_0",
                        new[] { new SqlCommandParameter("param_0", 41, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.IntField >= 42)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0",
                        new[] { new SqlCommandParameter("param_0", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.NullableStringField).Where(it => it != null)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - reverse binary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.NullableStringField).Where(it => null != it)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter after anonymous projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => new { it.BooleanField, it.StringField }).Where(it => it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property comparison",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.Enum > EnEnum.Two)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" > @param_0",
                        new[] { new SqlCommandParameter("param_0", EnEnum.Two, typeof(EnEnum)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property filter (equals operator)",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.Enum == EnEnum.Three)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" = @param_0",
                        new[] { new SqlCommandParameter("param_0", EnEnum.Three, typeof(EnEnum)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property filter (object.Equals)",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.Enum.Equals(EnEnum.Three))),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" = @param_0",
                        new[] { new SqlCommandParameter("param_0", EnEnum.Three, typeof(EnEnum)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property filter (array.Contains)",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => new[] { EnEnum.Three }.Contains(it.Enum))),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" = ANY(@param_0)",
                        new[] { new SqlCommandParameter("param_0", new[] { EnEnum.Three }, typeof(EnEnum[])) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property filter (list.Contains)",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => new List<EnEnum> { EnEnum.Three }.Contains(it.Enum))),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" = ANY(@param_0)",
                        new[] { new SqlCommandParameter("param_0", new List<EnEnum> { EnEnum.Three }, typeof(List<EnEnum>)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - change anonymous projection parameter name",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => new { Nsf = it.NullableStringField, Sf = it.StringField }).Where(it => it.Nsf != null)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" AS ""Nsf"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"" AS ""Sf""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - coalesce projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.NullableStringField ?? string.Empty)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}COALESCE(a.""{nameof(DatabaseEntity.NullableStringField)}"", @param_0){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new[] { new SqlCommandParameter("param_0", string.Empty, typeof(string)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection to anonymous type",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.BooleanField).Select(it => new { it.StringField }).Distinct()),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection to primitive type",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.BooleanField).Select(it => it.StringField).Distinct()),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with join expression",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<Post>().Where(it => it.Blog.Theme == "MilkyWay").Select(it => it.User.Nickname).Distinct()),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}d.""{nameof(Post.User.Nickname)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.Nickname)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Blog)}"" b{Environment.NewLine}{'\t'}JOIN{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}ON{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}CASE WHEN @param_0 IS NULL THEN b.""{nameof(Blog.Theme)}"" IS NULL ELSE b.""{nameof(Blog.Theme)}"" = @param_1 END) c{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(User.PrimaryKey)}"" = c.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""",
                        new[] { new SqlCommandParameter("param_0", "MilkyWay", typeof(string)), new SqlCommandParameter("param_1", "MilkyWay", typeof(string)) },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with predicate",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => new { it.StringField, it.BooleanField }).Where(it => it.BooleanField).Distinct()),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - bool",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - guid",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.PrimaryKey)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - int",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.IntField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - string",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.StringField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<Post>().Where(it => it.Blog.Theme == "MilkyWay" && it.User.Nickname == "SpaceEngineer")),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}b.""{nameof(Post.User.PrimaryKey)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(User.PrimaryKey)}"" = a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN c.""{nameof(Blog.Theme)}"" IS NULL ELSE c.""{nameof(Blog.Theme)}"" = @param_1 END AND CASE WHEN @param_2 IS NULL THEN b.""{nameof(User.Nickname)}"" IS NULL ELSE b.""{nameof(User.Nickname)}"" = @param_3 END",
                        new[] { new SqlCommandParameter("param_0", "MilkyWay", typeof(string)), new SqlCommandParameter("param_1", "MilkyWay", typeof(string)), new SqlCommandParameter("param_2", "SpaceEngineer", typeof(string)), new SqlCommandParameter("param_3", "SpaceEngineer", typeof(string)) },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection with filter as source",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<Post>().Where(it => it.DateTime > DateTime.MinValue).Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(Post.Blog.Theme)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.Theme)}"",{Environment.NewLine}{'\t'}c.""{nameof(User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"" > @param_0) b{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(User.PrimaryKey)}"" = b.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(Blog.PrimaryKey)}"" = b.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""",
                        new[] { new SqlCommandParameter("param_0", DateTime.MinValue, typeof(DateTime)) },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<Post>().Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.Theme)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.Theme)}"",{Environment.NewLine}{'\t'}b.""{nameof(User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(User.PrimaryKey)}"" = a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by join",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<Post>().OrderByDescending(it => it.Blog.Theme).ThenBy(it => it.User.Nickname)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}b.""{nameof(Post.User.PrimaryKey)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(User.PrimaryKey)}"" = a.""{nameof(post.User)}_{nameof(post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(post.Blog)}_{nameof(post.Blog.PrimaryKey)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}c.""{nameof(Blog.Theme)}"" DESC, b.""{nameof(User.Nickname)}"" ASC",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by then by",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.BooleanField).OrderBy(it => it.IntField).ThenByDescending(it => it.StringField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" ASC, a.""{nameof(DatabaseEntity.StringField)}"" DESC",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.BooleanField).OrderBy(it => it.IntField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" ASC",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - projection/filter chain",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => new { it.NullableStringField, it.StringField, it.IntField }).Select(it => new { it.NullableStringField, it.IntField }).Where(it => it.NullableStringField != null).Select(it => new { it.IntField }).Where(it => it.IntField > 0).Where(it => it.IntField <= 42).Select(it => it.IntField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a) b{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}CASE WHEN @param_0 IS NULL THEN b.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE b.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END) c{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}"" > @param_2 AND c.""{nameof(DatabaseEntity.IntField)}"" <= @param_3) d",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", 0, typeof(int)), new SqlCommandParameter("param_3", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - property chain with translated member",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.StringField.Length)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}length(a.""{nameof(DatabaseEntity.StringField)}""){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - all",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().All(it => it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(CASE WHEN a.""{nameof(DatabaseEntity.BooleanField)}"" THEN @param_0 ELSE NULL END) = Count(*)) AS ""All""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new[] { new SqlCommandParameter("param_0", 1, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - any",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Any(it => it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*) > @param_0) AS ""Any""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        new[] { new SqlCommandParameter("param_0", 0, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - count",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Count(it => it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*)) AS ""Count""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().First(it => it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}fetch first 1 rows only",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().Single<DatabaseEntity, Guid>(testDatabaseEntity.PrimaryKey, CancellationToken.None)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new[] { new SqlCommandParameter("param_0", testDatabaseEntity.PrimaryKey, typeof(Guid)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().SingleOrDefault<DatabaseEntity, Guid>(testDatabaseEntity.PrimaryKey, CancellationToken.None)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}fetch first 2 rows only",
                        new[] { new SqlCommandParameter("param_0", testDatabaseEntity.PrimaryKey, typeof(Guid)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Single(it => it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}fetch first 2 rows only",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sub-query",
                new Func<IDependencyContainer, object?>(container =>
                {
                    var subQuery = container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.PrimaryKey);
                    return container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => subQuery.Contains(it.PrimaryKey));
                }),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = ANY(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a)",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sub-query with parameters",
                new Func<IDependencyContainer, object?>(container =>
                {
                    var subQuery = container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.BooleanField == true).Select(it => it.PrimaryKey);
                    return container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.NullableStringField != null && subQuery.Contains(it.PrimaryKey));
                }),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END AND a.""{nameof(DatabaseEntity.PrimaryKey)}"" = ANY(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"" = @param_2) b)",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", true, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection with renaming",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => new { it.StringField, Filter = it.NullableStringField }).Where(it => it.Filter != null ? true : false)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" AS ""Filter""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END THEN @param_2 ELSE @param_3 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", true, typeof(bool)), new SqlCommandParameter("param_3", false, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => new { it.StringField, it.NullableStringField }).Where(it => it.NullableStringField != null ? true : false)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END THEN @param_2 ELSE @param_3 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", true, typeof(bool)), new SqlCommandParameter("param_3", false, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => it.NullableStringField != null ? true : false)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END THEN @param_2 ELSE @param_3 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", true, typeof(bool)), new SqlCommandParameter("param_3", false, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => it.NullableStringField != null ? it.NullableStringField : string.Empty)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}CASE WHEN CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" ELSE @param_2 END{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", string.Empty, typeof(string)) },
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Where(it => !it.BooleanField || it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}"" OR a.""{nameof(DatabaseEntity.BooleanField)}""",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection to anonymous class",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => new { Negation = !it.BooleanField })),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(NOT a.""{nameof(DatabaseEntity.BooleanField)}"") AS ""Negation""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseEntity>().Select(it => !it.BooleanField)),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        emptyQueryParameters,
                        log)),
                new IDatabaseEntity[] { testDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sql view translation after migration",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseColumn>().Select(it => new { it.Schema, it.Table, it.Column }).First()),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Schema)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Table)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Column)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{nameof(DataAccess.Orm.Sql.Host.Migrations)}"".""{nameof(DatabaseColumn)}"" a{Environment.NewLine}fetch first 1 rows only",
                        emptyQueryParameters,
                        log)),
                Array.Empty<IDatabaseEntity>()
            };
            /*TODO: #209 - test sql view translation before migration*/
            /*yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sql view translation before migration",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IReadRepository>().All<DatabaseColumn>().Select(it => new { it.Schema, it.Table, it.Column }).First()),
                new Action<ICommand, Action<string>>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Schema)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Table)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Column)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}({Environment.NewLine}{'\t'}{'\t'}select{Environment.NewLine}{'\t'}{'\t'}gen_random_uuid() as ""{nameof(DatabaseColumn.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}c.table_schema as ""{nameof(DatabaseColumn.Schema)}"",{Environment.NewLine}{'\t'}{'\t'}c.table_name as ""{nameof(DatabaseColumn.Table)}"",{Environment.NewLine}{'\t'}{'\t'}column_name as ""{nameof(DatabaseColumn.Column)}"",{Environment.NewLine}{'\t'}{'\t'}ordinal_position as ""{nameof(DatabaseColumn.Position)}"",{Environment.NewLine}{'\t'}{'\t'}data_type as ""{nameof(DatabaseColumn.DataType)}"",{Environment.NewLine}{'\t'}{'\t'}case is_nullable when 'NO' then false when 'YES' then true end as ""{nameof(DatabaseColumn.Nullable)}"",{Environment.NewLine}{'\t'}{'\t'}column_default as ""{nameof(DatabaseColumn.DefaultValue)}"",{Environment.NewLine}{'\t'}{'\t'}numeric_scale as ""{nameof(DatabaseColumn.Scale)}"",{Environment.NewLine}{'\t'}{'\t'}numeric_precision as ""{nameof(DatabaseColumn.Precision)}"",{Environment.NewLine}{'\t'}{'\t'}character_maximum_length as ""{nameof(DatabaseColumn.Length)}""{Environment.NewLine}{'\t'}{'\t'}from information_schema.columns c{Environment.NewLine}{'\t'}{'\t'}join information_schema.tables t{Environment.NewLine}{'\t'}{'\t'}on t.table_schema = c.table_schema and t.table_name = c.table_name  {Environment.NewLine}{'\t'}{'\t'}where t.table_type != 'VIEW' and c.table_schema not in ('information_schema', 'public') and c.table_schema not like 'pg_%'{Environment.NewLine}{'\t'}{'\t'}order by c.table_name, ordinal_position{Environment.NewLine}{'\t'}) a{Environment.NewLine}fetch first 1 rows only",
                        emptyQueryParameters,
                        log)),
                Array.Empty<IDatabaseEntity>()
            };*/
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
        [MemberData(nameof(CommandTranslationTestData))]
        internal async Task CommandTranslationTest(
            Lazy<IHost> host,
            CancellationTokenSource cts,
            AsyncCountdownEvent asyncCountdownEvent,
            string section,
            Func<IDependencyContainer, object?> queryProducer,
            Action<ICommand, Action<string>> checkQuery,
            IDatabaseEntity[] entities)
        {
            try
            {
                Output.WriteLine(section);

                var endpointDependencyContainer = host.Value.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                var sqlDatabaseSettings = await endpointDependencyContainer
                   .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(nameof(CommandTranslationTest), sqlDatabaseSettings.Database);
                Assert.Equal(IsolationLevel.ReadCommitted, sqlDatabaseSettings.IsolationLevel);
                Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                var ormSettings = await endpointDependencyContainer
                    .Resolve<ISettingsProvider<OrmSettings>>()
                    .Get(cts.Token)
                    .ConfigureAwait(false);

                Assert.Equal(10u, ormSettings.CommandSecondsTimeout);

                var hostShutdown = host.Value.WaitForShutdownAsync(cts.Token);

                var assert = Run(endpointDependencyContainer, queryProducer, checkQuery, entities, Output.WriteLine, cts.Token);

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

        private static Task Run(
            IDependencyContainer dependencyContainer,
            Func<IDependencyContainer, object?> queryProducer,
            Action<ICommand, Action<string>> checkQuery,
            IDatabaseEntity[] entities,
            Action<string> log,
            CancellationToken token)
        {
            return dependencyContainer.InvokeWithinTransaction(false, RunWithinTransaction, token);

            async Task RunWithinTransaction(IAdvancedDatabaseTransaction transaction, CancellationToken token)
            {
                var collector = dependencyContainer.Resolve<QueryExpressionsCollector>();
                collector.Expressions.Clear();

                var expression = (queryProducer(dependencyContainer) as IQueryable)?.Expression
                                 ?? collector.Expressions.Single();

                var query = dependencyContainer
                    .Resolve<IQueryProvider>()
                    .CreateQuery(expression);

                var command = dependencyContainer
                    .Resolve<IExpressionTranslator>()
                    .Translate(expression);

                checkQuery(command, log);

                await transaction
                    .Insert(entities, EnInsertBehavior.Default, token)
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
            }
        }

        private static void CheckSqlCommand(
            ICommand command,
            string expectedQuery,
            IReadOnlyCollection<SqlCommandParameter> expectedQueryParameters,
            Action<string> log)
        {
            var sqlCommand = (SqlCommand)command;

            log("Expected query:");
            log(expectedQuery);

            log("Actual query:");
            log(sqlCommand.CommandText);

            log("Expected parameters:");
            log(FormatParameters(expectedQueryParameters));

            log("Actual parameters:");
            log(FormatParameters(sqlCommand.CommandParameters));

            Assert.Equal(expectedQuery, sqlCommand.CommandText, StringComparer.Ordinal);
            CheckParameters(sqlCommand.CommandParameters, expectedQueryParameters);
        }

        private static string FormatParameters(IReadOnlyCollection<SqlCommandParameter> queryParameters)
        {
            return queryParameters.Any()
                ? queryParameters.ToString(" ")
                : "empty parameters";
        }

        private static void CheckParameters(
            IReadOnlyCollection<SqlCommandParameter> actualQueryParameters,
            IReadOnlyCollection<SqlCommandParameter> expectedQueryParameters)
        {
            var parameters = actualQueryParameters
                .FullOuterJoin(expectedQueryParameters,
                    actual => actual.Name,
                    expected => expected.Name,
                    (actual, expected) => (actual, expected),
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var (actual, expected) in parameters)
            {
                Assert.Equal(expected?.Value, actual?.Value);
                Assert.Equal(expected?.Type, actual?.Type);
            }
        }
    }
}