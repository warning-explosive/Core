namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthEndpoint.Domain;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Transaction;
    using DataAccess.Orm.Sql.Translation;
    using DatabaseEntities;
    using DatabaseEntities.Relations;
    using GenericDomain.EventSourcing.Sql;
    using GenericEndpoint.DataAccess.Sql.Host;
    using GenericEndpoint.Host;
    using GenericHost;
    using IntegrationTransport.Host;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using StartupActions;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// TranslationTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1505", Justification = "sql test cases")]
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    [SuppressMessage("Analysis", "SA1131", Justification = "test case")]
    public class TranslationTest : TestBase
    {
        private static TestFixture? _staticFixture;

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public TranslationTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            _staticFixture = fixture;
        }

        private static TestFixture StaticFixture => _staticFixture ?? throw new InvalidOperationException(nameof(_staticFixture));

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
            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory wasn't found");

            var settingsDirectory = projectFileDirectory
                .StepInto("Settings")
                .StepInto(nameof(CommandTranslationTest));

            var timeout = TimeSpan.FromSeconds(60);

            var cts = new CancellationTokenSource(timeout);

            var host = new Lazy<IHost>(() =>
                {
                    var hostBuilder = StaticFixture.CreateHostBuilder();

                    var databaseEntities = new[]
                    {
                        typeof(DatabaseDomainEvent),
                        typeof(DatabaseEntity),
                        typeof(ComplexDatabaseEntity),
                        typeof(Blog),
                        typeof(Post),
                        typeof(DatabaseEntities.Relations.User),
                        typeof(Community),
                        typeof(Participant)
                    };

                    var startupActions = new[]
                    {
                        typeof(CreateOrGetExistedPostgreSqlDatabaseHostStartupAction)
                    };

                    var additionalOurTypes = databaseEntities
                        .Concat(startupActions)
                        .ToArray();

                    var manualRegistrations = new IManualRegistration[]
                    {
                        new QueryExpressionsCollectorManualRegistration()
                    };

                    var host = hostBuilder
                       .UseIntegrationTransport((_, builder) => builder
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
            var schema = nameof(GenericHost) + nameof(Test);

            var databaseEntity = DatabaseEntity.Generate();

            var aggregateId = Guid.NewGuid();
            var username = "SpaceEngineer";
            var password = "12345678";
            var salt = Password.GenerateSalt();
            var passwordHash = new Password(password).GeneratePasswordHash(salt);
            var domainEvent = new DatabaseDomainEvent(
                Guid.NewGuid(),
                aggregateId,
                0,
                DateTime.UtcNow,
                new UserWasCreated(aggregateId, new Username(username), salt, passwordHash));

            var user = new DatabaseEntities.Relations.User(Guid.NewGuid(), "SpaceEngineer");
            var posts = new List<Post>();
            var blog = new Blog(Guid.NewGuid(), "MilkyWay", posts);
            var post = new Post(Guid.NewGuid(), blog, user, DateTime.Now, "PostContent");
            posts.Add(post);

            var message = new Command(42);

            var complexDatabaseEntity = ComplexDatabaseEntity.Generate(message, blog);
            var complexDatabaseEntityWithNulls = ComplexDatabaseEntity.GenerateWithNulls(message, blog);

            var token = CancellationToken.None;

            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - all",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - anonymous projections chain",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => new { it.StringField, it.IntField, it.BooleanField }).Select(it => new { it.StringField, it.IntField }).Select(it => new { it.IntField }).Select(it => it.IntField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a) b) c) d",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison !=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.IntField != 43)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" != @param_0",
                        new[] { new SqlCommandParameter("param_0", 43, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.IntField < 43)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" < @param_0",
                        new[] { new SqlCommandParameter("param_0", 43, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison <=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.IntField <= 42)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" <= @param_0",
                        new[] { new SqlCommandParameter("param_0", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison ==",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.IntField == 42)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" = @param_0",
                        new[] { new SqlCommandParameter("param_0", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.IntField > 41)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" > @param_0",
                        new[] { new SqlCommandParameter("param_0", 41, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary comparison >=",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.IntField >= 42)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" >= @param_0",
                        new[] { new SqlCommandParameter("param_0", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - binary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.NullableStringField).Where(it => it != null)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - reverse binary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.NullableStringField).Where(it => null != it)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter after anonymous projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => new { it.BooleanField, it.StringField }).Where(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - boolean property filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property comparison",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.Enum > EnEnum.Two)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" > @param_0",
                        new[] { new SqlCommandParameter("param_0", EnEnum.Two, typeof(EnEnum)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property filter (equals operator)",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.Enum == EnEnum.Three)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" = @param_0",
                        new[] { new SqlCommandParameter("param_0", EnEnum.Three, typeof(EnEnum)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property filter (object.Equals)",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.Enum.Equals(EnEnum.Three))),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" = @param_0",
                        new[] { new SqlCommandParameter("param_0", EnEnum.Three, typeof(EnEnum)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property filter (array.Contains)",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => new[] { EnEnum.Three }.Contains(it.Enum))),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" = ANY(@param_0)",
                        new[] { new SqlCommandParameter("param_0", new[] { EnEnum.Three }, typeof(EnEnum[])) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - enum property filter (list.Contains)",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => new List<EnEnum> { EnEnum.Three }.Contains(it.Enum))),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"" = ANY(@param_0)",
                        new[] { new SqlCommandParameter("param_0", new List<EnEnum> { EnEnum.Three }, typeof(List<EnEnum>)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - change anonymous projection parameter name",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => new { Nsf = it.NullableStringField, Sf = it.StringField }).Where(it => it.Nsf != null)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" AS ""Nsf"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"" AS ""Sf""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - coalesce projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.NullableStringField ?? string.Empty)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}COALESCE(a.""{nameof(DatabaseEntity.NullableStringField)}"", @param_0){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new[] { new SqlCommandParameter("param_0", string.Empty, typeof(string)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection to anonymous type",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).Select(it => new { it.StringField }).Distinct()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection to primitive type",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).Select(it => it.StringField).Distinct()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with join expression",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<Post>().Where(it => it.Blog.Theme == "MilkyWay").Select(it => it.User.Nickname).Distinct()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}d.""{nameof(Post.User.Nickname)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.Nickname)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntities.Relations.User)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Blog)}"" b{Environment.NewLine}{'\t'}JOIN{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}ON{Environment.NewLine}{'\t'}{'\t'}b.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}CASE WHEN @param_0 IS NULL THEN b.""{nameof(Blog.Theme)}"" IS NULL ELSE b.""{nameof(Blog.Theme)}"" = @param_1 END) c{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntities.Relations.User.PrimaryKey)}"" = c.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""",
                        new[] { new SqlCommandParameter("param_0", "MilkyWay", typeof(string)), new SqlCommandParameter("param_1", "MilkyWay", typeof(string)) },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - distinct projection with predicate",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => new { it.StringField, it.BooleanField }).Where(it => it.BooleanField).Distinct()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT DISTINCT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - bool",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - guid",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.PrimaryKey)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - int",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.IntField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one property projection - string",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.StringField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<Post>().Where(it => it.Blog.Theme == "MilkyWay" && it.User.Nickname == "SpaceEngineer")),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}b.""{nameof(Post.User.PrimaryKey)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntities.Relations.User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntities.Relations.User.PrimaryKey)}"" = a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN c.""{nameof(Blog.Theme)}"" IS NULL ELSE c.""{nameof(Blog.Theme)}"" = @param_1 END AND CASE WHEN @param_2 IS NULL THEN b.""{nameof(DatabaseEntities.Relations.User.Nickname)}"" IS NULL ELSE b.""{nameof(DatabaseEntities.Relations.User.Nickname)}"" = @param_3 END",
                        new[] { new SqlCommandParameter("param_0", "MilkyWay", typeof(string)), new SqlCommandParameter("param_1", "MilkyWay", typeof(string)), new SqlCommandParameter("param_2", "SpaceEngineer", typeof(string)), new SqlCommandParameter("param_3", "SpaceEngineer", typeof(string)) },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection with filter as source",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<Post>().Where(it => it.DateTime > DateTime.MinValue).Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(Post.Blog.Theme)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.Theme)}"",{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntities.Relations.User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" d{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntities.Relations.User)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(Post.DateTime)}"" > @param_0) b{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(DatabaseEntities.Relations.User.PrimaryKey)}"" = b.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}d.""{nameof(Blog.PrimaryKey)}"" = b.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""",
                        new[] { new SqlCommandParameter("param_0", DateTime.MinValue, typeof(DateTime)) },
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - one-to-one relation in projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<Post>().Select(it => new { it.Blog.Theme, Author = it.User.Nickname })),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.Theme)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.Theme)}"",{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntities.Relations.User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntities.Relations.User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntities.Relations.User.PrimaryKey)}"" = a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by join",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<Post>().OrderByDescending(it => it.Blog.Theme).ThenBy(it => it.User.Nickname)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.PrimaryKey)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.DateTime)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(Post.Text)}"",{Environment.NewLine}{'\t'}b.""{nameof(Post.User.PrimaryKey)}"" AS ""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntities.Relations.User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntities.Relations.User.PrimaryKey)}"" = a.""{nameof(post.User)}_{nameof(post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(post.Blog)}_{nameof(post.Blog.PrimaryKey)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}c.""{nameof(Blog.Theme)}"" DESC, b.""{nameof(DatabaseEntities.Relations.User.Nickname)}"" ASC",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by then by",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).OrderBy(it => it.IntField).ThenByDescending(it => it.StringField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" ASC, a.""{nameof(DatabaseEntity.StringField)}"" DESC",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - order by",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).OrderBy(it => it.IntField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}ORDER BY{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"" ASC",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - projection/filter chain",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => new { it.NullableStringField, it.StringField, it.IntField }).Select(it => new { it.NullableStringField, it.IntField }).Where(it => it.NullableStringField != null).Select(it => new { it.IntField }).Where(it => it.IntField > 0).Where(it => it.IntField <= 42).Select(it => it.IntField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}d.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}b.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a) b{Environment.NewLine}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}CASE WHEN @param_0 IS NULL THEN b.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE b.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END) c{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}c.""{nameof(DatabaseEntity.IntField)}"" > @param_2 AND c.""{nameof(DatabaseEntity.IntField)}"" <= @param_3) d",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", 0, typeof(int)), new SqlCommandParameter("param_3", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - property chain with translated member",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.StringField.Length)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}length(a.""{nameof(DatabaseEntity.StringField)}""){Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - all",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().All(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(CASE WHEN a.""{nameof(DatabaseEntity.BooleanField)}"" THEN @param_0 ELSE NULL END) = Count(*)) AS ""All""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new[] { new SqlCommandParameter("param_0", 1, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - all async",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().CachedExpression(Guid.NewGuid().ToString()).AllAsync(it => it.BooleanField, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(CASE WHEN a.""{nameof(DatabaseEntity.BooleanField)}"" THEN @param_0 ELSE NULL END) = Count(*)) AS ""All""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new[] { new SqlCommandParameter("param_0", 1, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - any",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).Any()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*) > @param_0) AS ""Any""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        new[] { new SqlCommandParameter("param_0", 0, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - any 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Any(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*) > @param_0) AS ""Any""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        new[] { new SqlCommandParameter("param_0", 0, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - any async",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).CachedExpression(Guid.NewGuid().ToString()).AnyAsync(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*) > @param_0) AS ""Any""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        new[] { new SqlCommandParameter("param_0", 0, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - any async 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().CachedExpression(Guid.NewGuid().ToString()).AnyAsync(it => it.BooleanField, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*) > @param_0) AS ""Any""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        new[] { new SqlCommandParameter("param_0", 0, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - count",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).Count()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*)) AS ""Count""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - count 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Count(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*)) AS ""Count""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - count async",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).CachedExpression(Guid.NewGuid().ToString()).CountAsync(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*)) AS ""Count""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - count async 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().CachedExpression(Guid.NewGuid().ToString()).CountAsync(it => it.BooleanField, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(Count(*)) AS ""Count""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"") b",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().First(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).First()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first async",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().CachedExpression(Guid.NewGuid().ToString()).FirstAsync(it => it.BooleanField, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first async 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).CachedExpression(Guid.NewGuid().ToString()).FirstAsync(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first or default",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().FirstOrDefault(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first or default 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).FirstOrDefault()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first or default async",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().CachedExpression(Guid.NewGuid().ToString()).FirstOrDefaultAsync(it => it.BooleanField, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - first or default async 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).CachedExpression(Guid.NewGuid().ToString()).FirstOrDefaultAsync(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Single(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).Single()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single async",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().CachedExpression(Guid.NewGuid().ToString()).SingleAsync(it => it.BooleanField, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single async 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).CachedExpression(Guid.NewGuid().ToString()).SingleAsync(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().SingleOrDefault(it => it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).SingleOrDefault()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default async",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().CachedExpression(Guid.NewGuid().ToString()).SingleOrDefaultAsync(it => it.BooleanField, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default async 2",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField).CachedExpression(Guid.NewGuid().ToString()).SingleOrDefaultAsync(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().Single<DatabaseEntity, Guid>(databaseEntity.PrimaryKey, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        new[] { new SqlCommandParameter("param_0", databaseEntity.PrimaryKey, typeof(Guid)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - scalar result - single or default by primary key",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().SingleOrDefault<DatabaseEntity, Guid>(databaseEntity.PrimaryKey, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = @param_0{Environment.NewLine}FETCH FIRST 2 ROWS ONLY",
                        new[] { new SqlCommandParameter("param_0", databaseEntity.PrimaryKey, typeof(Guid)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sub-query",
                new Func<IDependencyContainer, object?>(container =>
                {
                    var subQuery = container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.PrimaryKey);
                    return container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => subQuery.Contains(it.PrimaryKey));
                }),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"" = ANY(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a)",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sub-query with parameters",
                new Func<IDependencyContainer, object?>(container =>
                {
                    var subQuery = container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.BooleanField == true).Select(it => it.PrimaryKey);
                    return container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.NullableStringField != null && subQuery.Contains(it.PrimaryKey));
                }),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END AND a.""{nameof(DatabaseEntity.PrimaryKey)}"" = ANY(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}""{Environment.NewLine}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}{'\t'}{'\t'}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}{'\t'}{'\t'}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}{'\t'}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"" = @param_2) b)",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", true, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection with renaming",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => new { it.StringField, Filter = it.NullableStringField }).Where(it => it.Filter != null ? true : false)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"" AS ""Filter""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END THEN @param_2 ELSE @param_3 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", true, typeof(bool)), new SqlCommandParameter("param_3", false, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter after projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => new { it.StringField, it.NullableStringField }).Where(it => it.NullableStringField != null ? true : false)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END THEN @param_2 ELSE @param_3 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", true, typeof(bool)), new SqlCommandParameter("param_3", false, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => it.NullableStringField != null ? true : false)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END THEN @param_2 ELSE @param_3 END",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", true, typeof(bool)), new SqlCommandParameter("param_3", false, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - ternary projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => it.NullableStringField != null ? it.NullableStringField : string.Empty)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}CASE WHEN CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" IS NOT NULL ELSE a.""{nameof(DatabaseEntity.NullableStringField)}"" != @param_1 END THEN a.""{nameof(DatabaseEntity.NullableStringField)}"" ELSE @param_2 END{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        new[] { new SqlCommandParameter("param_0", default(string), typeof(string)), new SqlCommandParameter("param_1", default(string), typeof(string)), new SqlCommandParameter("param_2", string.Empty, typeof(string)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary filter",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Where(it => !it.BooleanField || it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.BooleanField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.IntField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.NullableStringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.StringField)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}"" OR a.""{nameof(DatabaseEntity.BooleanField)}""",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection to anonymous class",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => new { Negation = !it.BooleanField })),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}(NOT a.""{nameof(DatabaseEntity.BooleanField)}"") AS ""Negation""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - unary projection",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseEntity>().Select(it => !it.BooleanField)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}NOT a.""{nameof(DatabaseEntity.BooleanField)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sql view translation after migration",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseColumn>().Where(column => column.Schema == schema).First()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Column)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.DataType)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.DefaultValue)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Length)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Nullable)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Position)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Precision)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Scale)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Schema)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Table)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{nameof(DataAccess.Orm.Sql.Host.Migrations)}"".""{nameof(DatabaseColumn)}"" a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseColumn.Schema)}"" IS NULL ELSE a.""{nameof(DatabaseColumn.Schema)}"" = @param_1 END{Environment.NewLine}FETCH FIRST 1 ROWS ONLY",
                        new[] { new SqlCommandParameter("param_0", schema, typeof(string)), new SqlCommandParameter("param_1", schema, typeof(string)) },
                        log)),
                Array.Empty<IDatabaseEntity>()
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - sql view translation before migration (cachekey = 'C3B9DD2E-7279-455D-A718-356FD8F86035')",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseColumn>().Where(column => column.Schema == schema).CachedExpression("C3B9DD2E-7279-455D-A718-356FD8F86035").ToListAsync(token).Result.First()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Column)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.DataType)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.DefaultValue)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Length)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Nullable)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Position)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Precision)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Scale)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Schema)}"",{Environment.NewLine}{'\t'}a.""{nameof(DatabaseColumn.Table)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}({Environment.NewLine}{'\t'}{'\t'}select{Environment.NewLine}{'\t'}{'\t'}gen_random_uuid() as ""{nameof(DatabaseColumn.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}c.table_schema as ""{nameof(DatabaseColumn.Schema)}"",{Environment.NewLine}{'\t'}{'\t'}c.table_name as ""{nameof(DatabaseColumn.Table)}"",{Environment.NewLine}{'\t'}{'\t'}column_name as ""{nameof(DatabaseColumn.Column)}"",{Environment.NewLine}{'\t'}{'\t'}ordinal_position as ""{nameof(DatabaseColumn.Position)}"",{Environment.NewLine}{'\t'}{'\t'}data_type as ""{nameof(DatabaseColumn.DataType)}"",{Environment.NewLine}{'\t'}{'\t'}case is_nullable when 'NO' then false when 'YES' then true end as ""{nameof(DatabaseColumn.Nullable)}"",{Environment.NewLine}{'\t'}{'\t'}column_default as ""{nameof(DatabaseColumn.DefaultValue)}"",{Environment.NewLine}{'\t'}{'\t'}numeric_scale as ""{nameof(DatabaseColumn.Scale)}"",{Environment.NewLine}{'\t'}{'\t'}numeric_precision as ""{nameof(DatabaseColumn.Precision)}"",{Environment.NewLine}{'\t'}{'\t'}character_maximum_length as ""{nameof(DatabaseColumn.Length)}""{Environment.NewLine}{'\t'}{'\t'}from information_schema.columns c{Environment.NewLine}{'\t'}{'\t'}join information_schema.tables t{Environment.NewLine}{'\t'}{'\t'}on t.table_schema = c.table_schema and t.table_name = c.table_name  {Environment.NewLine}{'\t'}{'\t'}where t.table_type != 'VIEW' and c.table_schema not in ('information_schema', 'public') and c.table_schema not like 'pg_%'{Environment.NewLine}{'\t'}{'\t'}order by c.table_name, ordinal_position{Environment.NewLine}{'\t'}) a{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}CASE WHEN @param_0 IS NULL THEN a.""{nameof(DatabaseColumn.Schema)}"" IS NULL ELSE a.""{nameof(DatabaseColumn.Schema)}"" = @param_1 END",
                        new[] { new SqlCommandParameter("param_0", schema, typeof(string)), new SqlCommandParameter("param_1", schema, typeof(string)) },
                        log)),
                Array.Empty<IDatabaseEntity>()
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - select json column",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseDomainEvent>().Where(it => it.AggregateId == aggregateId).Select(it => it.DomainEvent)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseDomainEvent.DomainEvent)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.AggregateId)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.DomainEvent)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Index)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Timestamp)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{nameof(GenericEndpoint.EventSourcing)}"".""{nameof(DatabaseDomainEvent)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.AggregateId)}"" = @param_0) b",
                        new[] { new SqlCommandParameter("param_0", aggregateId, typeof(Guid)) },
                        log)),
                new IDatabaseEntity[] { domainEvent }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - select json column to anonymous type",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseDomainEvent>().Where(it => it.AggregateId == aggregateId).Select(it => new { it.AggregateId, it.DomainEvent })),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseDomainEvent.AggregateId)}"",{Environment.NewLine}{'\t'}b.""{nameof(DatabaseDomainEvent.DomainEvent)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.AggregateId)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.DomainEvent)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Index)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Timestamp)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{nameof(GenericEndpoint.EventSourcing)}"".""{nameof(DatabaseDomainEvent)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.AggregateId)}"" = @param_0) b",
                        new[] { new SqlCommandParameter("param_0", aggregateId, typeof(Guid)) },
                        log)),
                new IDatabaseEntity[] { domainEvent }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - select json attribute",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseDomainEvent>().Where(it => it.DomainEvent.AsJsonObject().HasJsonAttribute(nameof(UserWasCreated.Username)) && it.DomainEvent.AsJsonObject().GetJsonAttribute<Username>(nameof(UserWasCreated.Username)).HasJsonAttribute(nameof(Username.Value)) && it.DomainEvent.AsJsonObject().GetJsonAttribute<Username>(nameof(UserWasCreated.Username)).GetJsonAttribute<string>(nameof(Username.Value)) == username.AsJsonObject()).Select(it => it.DomainEvent.AsJsonObject().GetJsonAttribute<Username>(nameof(UserWasCreated.Username)).GetJsonAttribute<string>(nameof(Username.Value)).Value)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}b.""{nameof(DatabaseDomainEvent.DomainEvent)}""[@param_6][@param_7]{Environment.NewLine}FROM{Environment.NewLine}{'\t'}(SELECT{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.AggregateId)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.DomainEvent)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Index)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.PrimaryKey)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Timestamp)}"",{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.Version)}""{Environment.NewLine}{'\t'}FROM{Environment.NewLine}{'\t'}{'\t'}""{nameof(GenericEndpoint.EventSourcing)}"".""{nameof(DatabaseDomainEvent)}"" a{Environment.NewLine}{'\t'}WHERE{Environment.NewLine}{'\t'}{'\t'}a.""{nameof(DatabaseDomainEvent.DomainEvent)}"" ? @param_0 AND a.""{nameof(DatabaseDomainEvent.DomainEvent)}""[@param_1] ? @param_2 AND a.""{nameof(DatabaseDomainEvent.DomainEvent)}""[@param_3][@param_4] = @param_5) b",
                        new[] { new SqlCommandParameter("param_0", nameof(UserWasCreated.Username), typeof(string)), new SqlCommandParameter("param_1", nameof(UserWasCreated.Username), typeof(string)), new SqlCommandParameter("param_2", nameof(Username.Value), typeof(string)), new SqlCommandParameter("param_3", nameof(UserWasCreated.Username), typeof(string)), new SqlCommandParameter("param_4", nameof(Username.Value), typeof(string)), new SqlCommandParameter("param_5", username.AsJsonObject(), typeof(DatabaseJsonObject<string>)), new SqlCommandParameter("param_6", nameof(UserWasCreated.Username), typeof(string)), new SqlCommandParameter("param_7", nameof(Username.Value), typeof(string)) },
                        log)),
                new IDatabaseEntity[] { domainEvent }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - compose json object",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<DatabaseDomainEvent>().Select(it => it.DomainEvent.AsJsonObject().ExcludeJsonAttribute("$type").ExcludeJsonAttribute(nameof(UserWasCreated.Salt)).ExcludeJsonAttribute(nameof(UserWasCreated.PasswordHash)).ExcludeJsonAttribute(nameof(UserWasCreated.Username)).ConcatJsonObjects<ComposedJsonObject>(it.DomainEvent.AsJsonObject().GetJsonAttribute<Username>(nameof(UserWasCreated.Username))).TypedValue)),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(DatabaseDomainEvent.DomainEvent)}"" - @param_0 - @param_1 - @param_2 - @param_3 || a.""{nameof(DatabaseDomainEvent.DomainEvent)}""[@param_4]{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{nameof(GenericEndpoint.EventSourcing)}"".""{nameof(DatabaseDomainEvent)}"" a",
                        new[] { new SqlCommandParameter("param_0", "$type", typeof(string)), new SqlCommandParameter("param_1", nameof(UserWasCreated.Salt), typeof(string)), new SqlCommandParameter("param_2", nameof(UserWasCreated.PasswordHash), typeof(string)), new SqlCommandParameter("param_3", nameof(UserWasCreated.Username), typeof(string)), new SqlCommandParameter("param_4", nameof(UserWasCreated.Username), typeof(string)) },
                        log)),
                new IDatabaseEntity[] { domainEvent }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - insert entity",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().Insert(new IDatabaseEntity[] { databaseEntity }, EnInsertBehavior.Default).CachedExpression(Guid.NewGuid().ToString()).Invoke(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"INSERT INTO ""{schema}"".""{nameof(DatabaseEntity)}"" (""{nameof(DatabaseEntity.BooleanField)}"", ""{nameof(DatabaseEntity.Enum)}"", ""{nameof(DatabaseEntity.IntField)}"", ""{nameof(DatabaseEntity.NullableStringField)}"", ""{nameof(DatabaseEntity.PrimaryKey)}"", ""{nameof(DatabaseEntity.StringField)}"", ""{nameof(DatabaseEntity.Version)}""){Environment.NewLine}VALUES{Environment.NewLine}(@param_0, @param_1, @param_2, @param_3, @param_4, @param_5, @param_6)",
                        new[] { new SqlCommandParameter("param_0", true, typeof(bool)), new SqlCommandParameter("param_1", EnEnum.Three, typeof(EnEnum)), new SqlCommandParameter("param_2", 42, typeof(int)), new SqlCommandParameter("param_3", "SomeNullableString", typeof(string)), new SqlCommandParameter("param_4", databaseEntity.PrimaryKey, typeof(Guid)), new SqlCommandParameter("param_5", "SomeString", typeof(string)), new SqlCommandParameter("param_6", 0L, typeof(long)) },
                        log)),
                Array.Empty<IDatabaseEntity>()
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - insert several vales",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().Insert(new IDatabaseEntity[] { databaseEntity, DatabaseEntity.Generate(aggregateId) }, EnInsertBehavior.Default).CachedExpression(Guid.NewGuid().ToString()).Invoke(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"INSERT INTO ""{schema}"".""{nameof(DatabaseEntity)}"" (""{nameof(DatabaseEntity.BooleanField)}"", ""{nameof(DatabaseEntity.Enum)}"", ""{nameof(DatabaseEntity.IntField)}"", ""{nameof(DatabaseEntity.NullableStringField)}"", ""{nameof(DatabaseEntity.PrimaryKey)}"", ""{nameof(DatabaseEntity.StringField)}"", ""{nameof(DatabaseEntity.Version)}""){Environment.NewLine}VALUES{Environment.NewLine}(@param_0, @param_1, @param_2, @param_3, @param_4, @param_5, @param_6),{Environment.NewLine}(@param_7, @param_8, @param_9, @param_10, @param_11, @param_12, @param_13)",
                        new[] { new SqlCommandParameter("param_0", true, typeof(bool)), new SqlCommandParameter("param_1", EnEnum.Three, typeof(EnEnum)), new SqlCommandParameter("param_2", 42, typeof(int)), new SqlCommandParameter("param_3", "SomeNullableString", typeof(string)), new SqlCommandParameter("param_4", databaseEntity.PrimaryKey, typeof(Guid)), new SqlCommandParameter("param_5", "SomeString", typeof(string)), new SqlCommandParameter("param_6", 0L, typeof(long)), new SqlCommandParameter("param_7", true, typeof(bool)), new SqlCommandParameter("param_8", EnEnum.Three, typeof(EnEnum)), new SqlCommandParameter("param_9", 42, typeof(int)), new SqlCommandParameter("param_10", "SomeNullableString", typeof(string)), new SqlCommandParameter("param_11", aggregateId, typeof(Guid)), new SqlCommandParameter("param_12", "SomeString", typeof(string)), new SqlCommandParameter("param_13", 0L, typeof(long)) },
                        log)),
                Array.Empty<IDatabaseEntity>()
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - insert entities",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().Insert(new IDatabaseEntity[] { user, blog, post }, EnInsertBehavior.Default).CachedExpression(Guid.NewGuid().ToString()).Invoke(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"INSERT INTO ""{schema}"".""{nameof(DatabaseEntities.Relations.User)}"" (""{nameof(DatabaseEntities.Relations.User.Nickname)}"", ""{nameof(DatabaseEntities.Relations.User.PrimaryKey)}"", ""{nameof(DatabaseEntities.Relations.User.Version)}""){Environment.NewLine}VALUES{Environment.NewLine}(@param_0, @param_1, @param_2);{Environment.NewLine}INSERT INTO ""{schema}"".""{nameof(Blog)}"" (""{nameof(Blog.PrimaryKey)}"", ""{nameof(Blog.Theme)}"", ""{nameof(Blog.Version)}""){Environment.NewLine}VALUES{Environment.NewLine}(@param_3, @param_4, @param_5);{Environment.NewLine}INSERT INTO ""{schema}"".""{nameof(Post)}"" (""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}"", ""{nameof(Post.DateTime)}"", ""{nameof(Post.PrimaryKey)}"", ""{nameof(Post.Text)}"", ""{nameof(Post.User)}_{nameof(Post.PrimaryKey)}"", ""{nameof(Post.Version)}""){Environment.NewLine}VALUES{Environment.NewLine}(@param_6, @param_7, @param_8, @param_9, @param_10, @param_11);{Environment.NewLine}INSERT INTO ""{schema}"".""{nameof(Blog)}_{nameof(Post)}"" (""{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}"", ""{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}""){Environment.NewLine}VALUES{Environment.NewLine}(@param_12, @param_13)",
                        new[] { new SqlCommandParameter("param_0", "SpaceEngineer", typeof(string)), new SqlCommandParameter("param_1", user.PrimaryKey, typeof(Guid)), new SqlCommandParameter("param_2", 0L, typeof(long)), new SqlCommandParameter("param_3", blog.PrimaryKey, typeof(Guid)), new SqlCommandParameter("param_4", blog.Theme, typeof(string)), new SqlCommandParameter("param_5", 0L, typeof(long)), new SqlCommandParameter("param_6", blog.PrimaryKey, typeof(Guid)), new SqlCommandParameter("param_7", post.DateTime, typeof(DateTime)), new SqlCommandParameter("param_8", post.PrimaryKey, typeof(Guid)), new SqlCommandParameter("param_9", post.Text, typeof(string)), new SqlCommandParameter("param_10", user.PrimaryKey, typeof(Guid)), new SqlCommandParameter("param_11", 0L, typeof(long)), new SqlCommandParameter("param_12", blog.PrimaryKey, typeof(Guid)), new SqlCommandParameter("param_13", post.PrimaryKey, typeof(Guid)) },
                        log)),
                Array.Empty<IDatabaseEntity>()
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - delete entity",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().Delete<DatabaseEntity>().Where(it => it.BooleanField).CachedExpression(Guid.NewGuid().ToString()).Invoke(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"DELETE FROM ""{schema}"".""{nameof(DatabaseEntity)}""{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}""{nameof(DatabaseEntity.BooleanField)}""",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - delete entity with query parameters",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().Delete<DatabaseEntity>().Where(it => it.BooleanField == true && it.IntField == 42).CachedExpression(Guid.NewGuid().ToString()).Invoke(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"DELETE FROM ""{schema}"".""{nameof(DatabaseEntity)}""{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}""{nameof(DatabaseEntity.BooleanField)}"" = @param_0 AND ""{nameof(DatabaseEntity.IntField)}"" = @param_1",
                        new[] { new SqlCommandParameter("param_0", true, typeof(bool)), new SqlCommandParameter("param_1", 42, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - update entity with column reference",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().Update<DatabaseEntity>().Set(it => it.IntField.Assign(it.IntField + 1)).Where(it => it.BooleanField).CachedExpression(Guid.NewGuid().ToString()).Invoke(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"UPDATE ""{schema}"".""{nameof(DatabaseEntity)}""{Environment.NewLine}SET ""{nameof(DatabaseEntity.IntField)}"" = ""{nameof(DatabaseEntity.IntField)}"" + @param_0{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}""{nameof(DatabaseEntity.BooleanField)}""",
                        new[] { new SqlCommandParameter("param_0", 1, typeof(int)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - update entity with multiple sets",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().Update<DatabaseEntity>().Set(it => it.IntField.Assign(it.IntField + 1)).Set(it => it.Enum.Assign(EnEnum.Two)).Where(it => it.BooleanField == true).CachedExpression(Guid.NewGuid().ToString()).Invoke(token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"UPDATE ""{schema}"".""{nameof(DatabaseEntity)}""{Environment.NewLine}SET ""{nameof(DatabaseEntity.IntField)}"" = ""{nameof(DatabaseEntity.IntField)}"" + @param_0, ""{nameof(DatabaseEntity.Enum)}"" = @param_1{Environment.NewLine}WHERE{Environment.NewLine}{'\t'}""{nameof(DatabaseEntity.BooleanField)}"" = @param_2",
                        new[] { new SqlCommandParameter("param_0", 1, typeof(int)), new SqlCommandParameter("param_1", EnEnum.Two, typeof(EnEnum)), new SqlCommandParameter("param_2", true, typeof(bool)) },
                        log)),
                new IDatabaseEntity[] { databaseEntity }
            };
            /*TODO: #backlog - test update/delete with join in predicate*/
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - explain analyze",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<Post>().Select(it => new { it.Blog.Theme, Author = it.User.Nickname }).CachedExpression(Guid.NewGuid().ToString()).Explain(true, token).Result),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"EXPLAIN (ANALYZE, FORMAT json){Environment.NewLine}SELECT{Environment.NewLine}{'\t'}c.""{nameof(Post.Blog.Theme)}"" AS ""{nameof(Post.Blog)}_{nameof(Post.Blog.Theme)}"",{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntities.Relations.User.Nickname)}"" AS ""Author""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Blog)}"" c{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(DatabaseEntities.Relations.User)}"" b{Environment.NewLine}JOIN{Environment.NewLine}{'\t'}""{schema}"".""{nameof(Post)}"" a{Environment.NewLine}ON{Environment.NewLine}{'\t'}b.""{nameof(DatabaseEntities.Relations.User.PrimaryKey)}"" = a.""{nameof(Post.User)}_{nameof(Post.User.PrimaryKey)}""{Environment.NewLine}ON{Environment.NewLine}{'\t'}c.""{nameof(Blog.PrimaryKey)}"" = a.""{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}""",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { user, blog, post }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - read\\write complex object (with relations, arrays, json, nullable columns) without null values",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<ComplexDatabaseEntity>()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.DateTimeArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.EnumArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.EnumFlags)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.Json)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableDateTimeArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableEnum)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableEnumArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableEnumFlags)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableJson)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableRelation)}_{nameof(complexDatabaseEntity.NullableRelation.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableString)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableStringArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.Relation)}_{nameof(complexDatabaseEntity.Relation.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.String)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.StringArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(ComplexDatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { complexDatabaseEntity }
            };
            yield return new object[]
            {
                $"{nameof(DataAccess.Orm.PostgreSql)} - read\\write complex object (with relations, arrays, json, nullable columns) with null values",
                new Func<IDependencyContainer, object?>(container => container.Resolve<IDatabaseContext>().All<ComplexDatabaseEntity>()),
                new Action<ICommand, ITestOutputHelper>(
                    (query, log) => CheckSqlCommand(query,
                        $@"SELECT{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.DateTimeArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.Enum)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.EnumArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.EnumFlags)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.Json)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableDateTimeArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableEnum)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableEnumArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableEnumFlags)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableJson)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableRelation)}_{nameof(complexDatabaseEntity.NullableRelation.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableString)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.NullableStringArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.Relation)}_{nameof(complexDatabaseEntity.Relation.PrimaryKey)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.String)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.StringArray)}"",{Environment.NewLine}{'\t'}a.""{nameof(ComplexDatabaseEntity.Version)}""{Environment.NewLine}FROM{Environment.NewLine}{'\t'}""{schema}"".""{nameof(ComplexDatabaseEntity)}"" a",
                        Array.Empty<SqlCommandParameter>(),
                        log)),
                new IDatabaseEntity[] { complexDatabaseEntityWithNulls }
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
        [MemberData(nameof(CommandTranslationTestData))]
        internal async Task CommandTranslationTest(
            Lazy<IHost> host,
            CancellationTokenSource cts,
            AsyncCountdownEvent asyncCountdownEvent,
            string section,
            Func<IDependencyContainer, object?> queryProducer,
            Action<ICommand, ITestOutputHelper> checkCommand,
            IDatabaseEntity[] entities)
        {
            try
            {
                Output.WriteLine(section);

                var endpointDependencyContainer = host.Value.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                var sqlDatabaseSettings = endpointDependencyContainer
                    .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                    .Get();

                Assert.Equal(nameof(CommandTranslationTest), sqlDatabaseSettings.Database);
                Assert.Equal(IsolationLevel.ReadCommitted, sqlDatabaseSettings.IsolationLevel);
                Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                var ormSettings = endpointDependencyContainer
                    .Resolve<ISettingsProvider<OrmSettings>>()
                    .Get();

                Assert.Equal(10u, ormSettings.CommandSecondsTimeout);

                var hostShutdown = host.Value.WaitForShutdownAsync(cts.Token);

                var assert = Run(endpointDependencyContainer, queryProducer, checkCommand, entities, Output, cts.Token);

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
            Action<ICommand, ITestOutputHelper> checkCommand,
            IDatabaseEntity[] entities,
            ITestOutputHelper output,
            CancellationToken token)
        {
            return dependencyContainer.InvokeWithinTransaction(false, RunWithinTransaction, token);

            async Task RunWithinTransaction(IAdvancedDatabaseTransaction transaction, CancellationToken token)
            {
                if (entities.Any())
                {
                    await transaction
                    .Insert(entities, EnInsertBehavior.Default)
                    .CachedExpression(Guid.NewGuid().ToString())
                    .Invoke(token)
                    .ConfigureAwait(false);
                }

                var collector = dependencyContainer.Resolve<QueryExpressionsCollector>();

                Expression expression;
                object? item;

                var queryOrItem = queryProducer(dependencyContainer);

                if (queryOrItem is IQueryable queryable)
                {
                    expression = queryable.Expression;

                    item = queryable
                        .GetEnumerator()
                        .AsEnumerable<object>()
                        .Single();
                }
                else
                {
                    expression = collector.Expressions.Last();

                    item = queryOrItem;
                }

                var command = dependencyContainer
                    .Resolve<IExpressionTranslator>()
                    .Translate(expression);

                checkCommand(command, output);

                Assert.NotNull(item);

                output.WriteLine("Dump:");

                output.WriteLine(item.Dump(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));
            }
        }

        private static void CheckSqlCommand(
            ICommand command,
            string expectedQuery,
            IReadOnlyCollection<SqlCommandParameter> expectedQueryParameters,
            ITestOutputHelper output)
        {
            var sqlCommand = (SqlCommand)command;

            output.WriteLine("Expected query:");
            output.WriteLine(expectedQuery);

            output.WriteLine("Actual query:");
            output.WriteLine(sqlCommand.CommandText);

            output.WriteLine("Expected parameters:");
            output.WriteLine(FormatParameters(expectedQueryParameters));

            output.WriteLine("Actual parameters:");
            output.WriteLine(FormatParameters(sqlCommand.CommandParameters));

            Assert.Equal(expectedQuery, sqlCommand.CommandText, StringComparer.Ordinal);
            CheckParameters(output, sqlCommand, expectedQueryParameters);
        }

        private static string FormatParameters(IReadOnlyCollection<SqlCommandParameter> queryParameters)
        {
            return queryParameters.Any()
                ? queryParameters.ToString(" ")
                : "empty parameters";
        }

        private static void CheckParameters(
            ITestOutputHelper output,
            SqlCommand command,
            IReadOnlyCollection<SqlCommandParameter> expectedQueryParameters)
        {
            var parameters = command
                .CommandParameters
                .FullOuterJoin(expectedQueryParameters,
                    actual => actual.Name,
                    expected => expected.Name,
                    (actual, expected) => (actual, expected),
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var (actual, expected) in parameters)
            {
                if (actual != null
                    && expected != null
                    && IsVersionParameter(output, command.CommandText, actual))
                {
                    Assert.NotEqual(0L, actual.Value);
                    Assert.Equal(expected.Type, actual.Type);
                }
                else
                {
                    Assert.Equal(expected?.Value, actual?.Value);
                    Assert.Equal(expected?.Type, actual?.Type);
                }
            }

            static bool IsVersionParameter(
                ITestOutputHelper output,
                string commandText,
                SqlCommandParameter parameter)
            {
                if (!commandText.Contains("INSERT INTO", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                output.WriteLine($"@{parameter.Name}");

                var parameterIndex = commandText.IndexOf($"@{parameter.Name}", StringComparison.OrdinalIgnoreCase);

                HashSet<string> commandParameters;
                {
                    var left = commandText[..parameterIndex].LastIndexOf("(", StringComparison.OrdinalIgnoreCase);
                    var right = commandText[parameterIndex..].IndexOf(")", StringComparison.OrdinalIgnoreCase);

                    commandParameters = commandText
                        .Substring(left + 1, parameterIndex - left + right - 1)
                        .Split(new[] { ",", " ", "\"", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    output.WriteLine(commandText.Substring(left + 1, parameterIndex - left + right - 1));
                }

                var insertIndex = commandText[..parameterIndex].LastIndexOf("INSERT INTO", StringComparison.OrdinalIgnoreCase);

                HashSet<string> columns;
                {
                    var left = commandText[insertIndex..].IndexOf("(", StringComparison.OrdinalIgnoreCase);
                    var right = commandText[insertIndex..].IndexOf(")", StringComparison.OrdinalIgnoreCase);

                    columns = commandText[insertIndex..]
                        .Substring(left + 1, right - left - 1)
                        .Split(new[] { ",", " ", "\"", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    output.WriteLine(commandText.Substring(left + 1, right - left - 1));
                }

                Assert.Equal(commandParameters.Count, columns.Count);
                Assert.Contains($"@{parameter.Name}", commandParameters);

                var columnName = commandParameters
                    .Zip(columns)
                    .ToDictionary(
                        pair => pair.First,
                        pair => pair.Second,
                        StringComparer.OrdinalIgnoreCase)[$"@{parameter.Name}"];

                return columnName.Equals(nameof(IDatabaseEntity.Version), StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}