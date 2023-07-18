namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;
    using CompositionRoot;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Transaction;
    using GenericEndpoint.Contract;
    using GenericEndpoint.DataAccess.Sql.Host;
    using GenericEndpoint.Host;
    using IntegrationTransport.Host;
    using Test.Api.ClassFixtures;
    using IHost = Microsoft.Extensions.Hosting.IHost;

    /// <summary>
    /// IDatabaseConnectionProvider query benchmark source
    /// </summary>
    [SimpleJob(RunStrategy.Throughput)]
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    [SuppressMessage("Analysis", "CA1001", Justification = "benchmark source")]
    public class DatabaseConnectionProviderBenchmarkSource
    {
        private CancellationTokenSource? _cts;
        private IHost? _host;
        private IDependencyContainer? _dependencyContainer;

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            var hostBuilder = new TestFixture().CreateHostBuilder();

            var solutionFileDirectory = SolutionExtensions.SolutionFile().Directory
                                        ?? throw new InvalidOperationException("Solution directory wasn't found");

            var settingsDirectory = solutionFileDirectory
                .StepInto(nameof(Benchmarks))
                .StepInto(AssembliesExtensions.BuildName(nameof(GenericHost), nameof(Benchmark)))
                .StepInto("Settings")
                .StepInto(nameof(DatabaseConnectionProviderBenchmarkSource));

            var databaseEntities = new[]
            {
                typeof(ComplexDatabaseEntity),
                typeof(Blog),
                typeof(Post),
                typeof(User),
                typeof(Community),
                typeof(Participant)
            };

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostStartupAction)
            };

            var additionalOurTypes = databaseEntities
                .Concat(startupActions)
                .ToArray();

            var endpointIdentity = new EndpointIdentity(
                nameof(DatabaseConnectionProviderBenchmarkSource),
                Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Unable to get entry assembly"));

            _host = hostBuilder
                .UseIntegrationTransport((_, builder) => builder
                    .WithInMemoryIntegrationTransport()
                    .BuildOptions())
                .UseEndpoint(endpointIdentity,
                    (_, builder) => builder
                    .WithPostgreSqlDataAccess(options => options
                        .ExecuteMigrations())
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions())
                .BuildHost(settingsDirectory);

            _host.StartAsync(_cts.Token).Wait(_cts.Token);

            _dependencyContainer = _host.GetEndpointDependencyContainer(nameof(DatabaseConnectionProviderBenchmarkSource));
        }

        /// <summary>
        /// GlobalCleanup
        /// </summary>
        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _host.StopAsync(_cts.Token).Wait();
            _host.Dispose();
            _cts.Dispose();
        }

        /// <summary>
        /// IterationSetup
        /// </summary>
        [IterationSetup]
        public void IterationSetup()
        {
            Insert().Wait(_cts.Token);
        }

        /// <summary>
        /// IterationCleanup
        /// </summary>
        [IterationCleanup]
        public void IterationCleanup()
        {
            Delete().Wait(_cts.Token);
        }

        /// <summary> Read </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(Read), Baseline = true)]
        public async Task Read()
        {
            await _dependencyContainer!
                .InvokeWithinTransaction(true, Producer, _cts.Token)
                .ConfigureAwait(false);

            static Task Producer(
                IDatabaseContext transaction,
                CancellationToken token)
            {
                return transaction
                    .All<ComplexDatabaseEntity>()
                    .CachedExpression("ECF915DD-4CDA-4B6C-ACF5-E497B418D813")
                    .ToListAsync(token);
            }
        }

        /// <summary> Insert </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(Insert))]
        public async Task Insert()
        {
            var user = new User(Guid.NewGuid(), "SpaceEngineer");
            var posts = new List<Post>();
            var blog = new Blog(Guid.NewGuid(), "MilkyWay", posts);
            var post = new Post(Guid.NewGuid(), blog, user, DateTime.Now, "PostContent");
            posts.Add(post);

            var communities = new List<Community>();
            var participants = new List<Participant>();
            var community = new Community(Guid.NewGuid(), "AmazingCommunity", participants);
            var participant = new Participant(Guid.NewGuid(), "RegularParticipant", communities);
            communities.Add(community);
            participants.Add(participant);

            var message = new Request();
            var complexDatabaseEntity = ComplexDatabaseEntity.Generate(message, blog);

            var entities = new IDatabaseEntity[]
            {
                user,
                blog,
                post,
                complexDatabaseEntity,
                community,
                participant
            };

            await _dependencyContainer!
                .InvokeWithinTransaction(true, entities, Producer, _cts.Token)
                .ConfigureAwait(false);

            static Task Producer(
                IDatabaseContext transaction,
                IDatabaseEntity[] entities,
                CancellationToken token)
            {
                return transaction
                    .Insert(entities, EnInsertBehavior.Default)
                    .CachedExpression("22046C75-F23F-4C21-B143-A98687105410")
                    .Invoke(token);
            }
        }

        /// <summary> Delete </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(Delete))]
        public async Task Delete()
        {
            await _dependencyContainer!
                .InvokeWithinTransaction(true, Producer, _cts.Token)
                .ConfigureAwait(false);

            static async Task Producer(
                IDatabaseContext transaction,
                CancellationToken token)
            {
                await transaction
                    .Delete<ComplexDatabaseEntity>()
                    .Where(_ => true)
                    .CachedExpression("14CBC111-F1B3-41AB-A816-47BF6C88B4EF")
                    .Invoke(token)
                    .ConfigureAwait(false);

                await transaction
                    .Delete<Blog>()
                    .Where(_ => true)
                    .CachedExpression("14A30D13-91D1-4CDA-8500-5F36C625B2F5")
                    .Invoke(token)
                    .ConfigureAwait(false);

                await transaction
                    .Delete<Post>()
                    .Where(_ => true)
                    .CachedExpression("C9A9FAA9-911E-4E7A-AF1F-34E16C36C168")
                    .Invoke(token)
                    .ConfigureAwait(false);

                await transaction
                    .Delete<User>()
                    .Where(_ => true)
                    .CachedExpression("D60B12D2-B738-4E86-BCAA-AE5D701FF8C0")
                    .Invoke(token)
                    .ConfigureAwait(false);

                await transaction
                    .Delete<Community>()
                    .Where(_ => true)
                    .CachedExpression("9379254D-97D6-4A96-854C-237220E4DD1E")
                    .Invoke(token)
                    .ConfigureAwait(false);

                await transaction
                    .Delete<Participant>()
                    .Where(_ => true)
                    .CachedExpression("248F4251-18A0-400B-BC94-62348F78DA71")
                    .Invoke(token)
                    .ConfigureAwait(false);
            }
        }
    }
}