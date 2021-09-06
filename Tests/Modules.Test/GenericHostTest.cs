namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Enumerations;
    using Basics.Primitives;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Exceptions;
    using CompositionRoot.Api.Extensions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.PostgreSql.Host;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Defaults;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Implementations;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.TestExtensions;
    using GenericHost;
    using GenericHost.Api.Abstractions;
    using InMemoryIntegrationTransport.Host;
    using InMemoryIntegrationTransport.Host.Implementations;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Api.Enumerations;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using Registrations;
    using StatisticsEndpoint.Contract.Messages;
    using Xunit;
    using Xunit.Abstractions;
    using IIntegrationContext = IntegrationTransport.Api.Abstractions.IIntegrationContext;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "For test reasons")]
    public class GenericHostTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public GenericHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// DependencyContainerTestData
        /// </summary>
        /// <returns>Dependency container producers</returns>
        public static IEnumerable<object[]> DependencyContainerTestData()
        {
            var dependencyContainerProducers = new object[]
            {
                new Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>(options => options.UseGenericContainer())
            };

            return new[] { dependencyContainerProducers };
        }

        /// <summary>
        /// useContainer; useTransport;
        /// </summary>
        /// <returns>BuildHostTestData</returns>
        public static IEnumerable<object[]> BuildHostTestData()
        {
            var integrationTransportProviders = new object[]
            {
                new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder.UseInMemoryIntegrationTransport()),
            };

            return DependencyContainerTestData()
                .SelectMany(it => integrationTransportProviders.Select(provider => it.Concat(new[] { provider }).ToArray()));
        }

        /// <summary>
        /// useContainer; useTransport; timeout;
        /// </summary>
        /// <returns>RunHostTestData</returns>
        public static IEnumerable<object[]> RunHostTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var collector = new MessagesCollector();
            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                .UseInMemoryIntegrationTransport(options => options
                    .WithManualRegistrations(new MessagesCollectorInstanceManualRegistration(collector))));

            var integrationTransportProviders = new object[]
            {
                useInMemoryIntegrationTransport
            };

            return DependencyContainerTestData()
                .SelectMany(it => integrationTransportProviders.Select(provider => it.Concat(new[] { provider, collector, timeout }).ToArray()));
        }

        /// <summary>
        /// useContainer; useTransport; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var databaseProviders = new object[]
            {
                new PostgreSqlDatabaseProvider()
            };

            return BuildHostTestData()
                .SelectMany(it => databaseProviders.Select(provider => it.Concat(new[] { provider, timeout }).ToArray()));
        }

        [Fact(Timeout = 60_000)]
        /*[MemberData(nameof(RunHostWithDataAccessTestData))]*/
        internal void BuildDatabaseModelTest()
        {
            /* TODO: #110
            var statisticsEndpointIdentity = new EndpointIdentity(StatisticsEndpointIdentity.LogicalName, 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseStatisticsEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(databaseProvider)
                    .BuildOptions(statisticsEndpointIdentity))
                .BuildHost();

            _ = timeout;
            return Task.CompletedTask;

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var container = host.GetEndpointDependencyContainer(statisticsEndpointIdentity);

                var actualModel = await container
                    .Resolve<IDatabaseModelBuilder>()
                    .BuildModel(cts.Token)
                    .ConfigureAwait(false);

                var expectedModel = await container
                    .Resolve<ICodeModelBuilder>()
                    .BuildModel(cts.Token)
                    .ConfigureAwait(false);

                var modelChanges = host
                    .GetEndpointDependencyContainer(statisticsEndpointIdentity)
                    .Resolve<IDatabaseModelComparator>()
                    .ExtractDiff(actualModel, expectedModel)
                    .ToList();

                modelChanges.Each(change => Output.WriteLine(change.ToString()));
                Assert.NotEmpty(modelChanges);

                var createDatabase = modelChanges.OfType<CreateDatabase>().Single();
                Assert.Equal("SpaceEngineersDatabase", createDatabase.Name);

                var upsertViewChanges = modelChanges.OfType<UpsertView>().ToList();
                Assert.NotNull(upsertViewChanges.SingleOrDefault(change => change.Type == typeof(DatabaseColumn)));
                Assert.NotNull(upsertViewChanges.SingleOrDefault(change => change.Type == typeof(DatabaseView)));

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
            */
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RequestReplyTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(RequestQueryCommand),
                typeof(Query),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(RequestQueryCommandHandler),
                typeof(QueryAlwaysReplyMessageHandler),
                typeof(ReplyEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var command = new RequestQueryCommand(42);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(command, cts.Token)
                    .ConfigureAwait(false);

                await collector.WaitUntilMessageIsNotReceived<Reply>(reply => reply.Id == command.Id).ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                collector.ShowErrorMessages(Output.WriteLine);
                collector.ShowMessages(Output.WriteLine);

                var expectedMessageTypes = new[]
                {
                    typeof(RequestQueryCommand),
                    typeof(Query),
                    typeof(Reply)
                };

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(expectedMessageTypes, collector.Messages.Select(message => message.Payload.GetType()).ToList());
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RpcRequestTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(Query),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(QueryAlwaysReplyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var query = new Query(42);

                var reply = await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .RpcRequest<Query, Reply>(query, cts.Token)
                    .ConfigureAwait(false);

                Assert.Equal(query.Id, reply.Id);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                collector.ShowErrorMessages(Output.WriteLine);
                collector.ShowMessages(Output.WriteLine);

                Assert.Empty(collector.ErrorMessages);

                var expectedMessages = new[]
                {
                    typeof(Query),
                    typeof(Reply)
                };

                Assert.Equal(expectedMessages, collector.Messages.Select(message => message.Payload.GetType()).ToList());
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EndpointCanHaveSeveralMessageHandlersPerMessage(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(Command)
            };

            var messageHandlerTypes = new[]
            {
                typeof(CommandEmptyMessageHandler),
                typeof(CommandThrowingMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var overrides = Fixture.DelegateOverride(container =>
            {
                container.Override<IRetryPolicy, DefaultRetryPolicy, RetryPolicyMock>(EnLifestyle.Singleton);
            });

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes)
                        .WithOverrides(overrides))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(new Command(42), cts.Token)
                    .ConfigureAwait(false);

                await collector
                    .WaitUntilErrorMessageIsNotReceived<Command>()
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Single(collector.ErrorMessages);
                Assert.Equal(42.ToString(CultureInfo.InvariantCulture), collector.ErrorMessages.Single().exception.Message);
                Assert.Equal(3, collector.ErrorMessages.Single().message.ReadHeader<RetryCounter>().Value);
                Assert.Equal(new[] { 0, 1, 2, 3 }, collector.Messages.Where(message => message.Payload is Command).Select(message => message.ReadHeader<RetryCounter>()?.Value ?? default(int)).ToList());
                Assert.Equal(4, collector.Messages.Count(message => message.Payload is Endpoint1HandlerInvoked handlerInvoked && handlerInvoked.HandlerType == typeof(CommandEmptyMessageHandler)));
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ContravariantMessageHandlerTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(PublishInheritedEventCommand),
                typeof(BaseEvent),
                typeof(InheritedEvent)
            };

            var messageHandlerTypes = new[]
            {
                typeof(PublishInheritedEventCommandHandler),
                typeof(IntegrationEventEmptyMessageHandler),
                typeof(BaseEventEmptyMessageHandler),
                typeof(InheritedEventEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(new PublishInheritedEventCommand(42), cts.Token)
                    .ConfigureAwait(false);

                await Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(invoked => invoked.HandlerType == typeof(IntegrationEventEmptyMessageHandler)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(invoked => invoked.HandlerType == typeof(BaseEventEmptyMessageHandler)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(invoked => invoked.HandlerType == typeof(InheritedEventEmptyMessageHandler)))
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                collector.ShowErrorMessages(Output.WriteLine);
                collector.ShowMessages(Output.WriteLine);

                Assert.Empty(collector.ErrorMessages);

                var expectedMessageTypes = new[]
                {
                    typeof(PublishInheritedEventCommand),
                    typeof(InheritedEvent)
                };

                Assert.Equal(expectedMessageTypes, collector.Messages.Take(2).Select(message => message.Payload.GetType()).ToList());
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ThrowingMessageHandlerTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var endpointIdentity = new EndpointIdentity(TestIdentity.Endpoint1, 0);

            var messageTypes = new[]
            {
                typeof(Command)
            };

            var messageHandlerTypes = new[]
            {
                typeof(CommandThrowingMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var overrides = Fixture.DelegateOverride(container =>
            {
                container.Override<IRetryPolicy, DefaultRetryPolicy, RetryPolicyMock>(EnLifestyle.Singleton);
            });

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithOverrides(overrides)
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(new Command(42), cts.Token)
                    .ConfigureAwait(false);

                await collector
                    .WaitUntilErrorMessageIsNotReceived<Command>()
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Single(collector.ErrorMessages);
                Assert.Equal(42.ToString(CultureInfo.InvariantCulture), collector.ErrorMessages.Single().exception.Message);
                Assert.Equal(3, collector.ErrorMessages.Single().message.ReadHeader<RetryCounter>()?.Value);
                Assert.Equal(new[] { 0, 1, 2, 3 }, collector.Messages.Select(message => message.ReadHeader<RetryCounter>()?.Value ?? default(int)).ToList());
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EventSubscriptionBetweenEndpointsTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var endpoint1MessageTypes = new[]
            {
                typeof(Event)
            };

            var endpoint1MessageHandlerTypes = new[]
            {
                typeof(EventEmptyMessageHandler)
            };

            var endpoint2MessageTypes = new[]
            {
                typeof(Event),
                typeof(PublishEventCommand)
            };

            var endpoint2MessageHandlerTypes = new[]
            {
                typeof(PublishEventCommandHandler),
                typeof(EventEmptyMessageHandler)
            };

            var endpoint1AdditionalOurTypes = endpoint1MessageTypes.Concat(endpoint1MessageHandlerTypes).ToArray();
            var endpoint2AdditionalOurTypes = endpoint2MessageTypes.Concat(endpoint2MessageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint11))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(endpoint2AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint20))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(new PublishEventCommand(42), cts.Token)
                    .ConfigureAwait(false);

                await Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Event>(),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(invoked => invoked.EndpointIdentity.Equals(TestIdentity.Endpoint10) || invoked.EndpointIdentity.Equals(TestIdentity.Endpoint11)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint2HandlerInvoked>(invoked => invoked.EndpointIdentity.Equals(TestIdentity.Endpoint20)))
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void SameTransportTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(TestIdentity.Endpoint20))
                .BuildHost();

            var transport = host.GetTransportDependencyContainer().Resolve<IIntegrationTransport>();

            var transportIsSame = new[]
                {
                    host.GetEndpointDependencyContainer(TestIdentity.Endpoint10),
                    host.GetEndpointDependencyContainer(TestIdentity.Endpoint20)
                }
                .Select(container => container.Resolve<IIntegrationTransport>())
                .All(endpointTransport => ReferenceEquals(transport, endpointTransport));

            Assert.True(transportIsSame);
        }

        [Fact(Timeout = 60_000)]
        internal void MessageHandlerTestExtensionsTest()
        {
            new CommandEmptyMessageHandler(TestIdentity.Endpoint10)
                .OnMessage(new Command(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            new CommandThrowingMessageHandler()
                .OnMessage(new Command(42))
                .ProducesNothing()
                .Throws<InvalidOperationException>(ex => ex.Message == "42")
                .Invoke();

            new EventEmptyMessageHandler(TestIdentity.Endpoint20)
                .OnMessage(new Event(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            new QueryOddReplyMessageHandler()
                .OnMessage(new Query(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            new QueryOddReplyMessageHandler()
                .OnMessage(new Query(43))
                .DoesNotSend<IIntegrationCommand>()
                .DoesNotPublish<IIntegrationEvent>()
                .DoesNotRequest<Query, Reply>()
                .Replies<Reply>(reply => reply.Id == 43)
                .DoesNotThrow()
                .Invoke();
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void BuildHostTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var endpointIdentity = new EndpointIdentity(TestIdentity.Endpoint1, 0);

            var messageTypes = new[]
            {
                typeof(BaseEvent),
                typeof(InheritedEvent),
                typeof(Command),
                typeof(OpenGenericHandlerCommand),
                typeof(Query),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(IntegrationEventEmptyMessageHandler),
                typeof(BaseEventEmptyMessageHandler),
                typeof(InheritedEventEmptyMessageHandler),
                typeof(CommandEmptyMessageHandler),
                typeof(OpenGenericCommandEmptyMessageHandler<>),
                typeof(QueryAlwaysReplyMessageHandler),
                typeof(ReplyEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            using (host)
            {
                CheckHost(host);
                CheckEndpoint(host, endpointIdentity, Output.WriteLine);
                CheckTransport(host, Output.WriteLine);
            }

            static void CheckHost(IHost host)
            {
                _ = host.Services.GetRequiredService<IHostedService>();

                var hostStartupActions = host
                    .Services
                    .GetServices<IHostStartupAction>()
                    .ToList();

                Assert.Single(hostStartupActions);
                var hostStartupAction = hostStartupActions.Single();

                Assert.Equal(typeof(GenericEndpointHostStartupAction), hostStartupAction.GetType());

                var hostBackgroundWorkers = host
                    .Services
                    .GetServices<IHostBackgroundWorker>()
                    .ToList();

                Assert.Single(hostBackgroundWorkers);
                var hostBackgroundWorker = hostBackgroundWorkers.Single();

                Assert.Equal(typeof(IntegrationTransportHostBackgroundWorker), hostBackgroundWorker.GetType());
            }

            static void CheckEndpoint(IHost host, EndpointIdentity endpointIdentity, Action<string> log)
            {
                var endpointDependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);
                var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command), new StringFormatterMock());

                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<Core.GenericEndpoint.Api.Abstractions.IIntegrationContext>());

                using (endpointDependencyContainer.OpenScope())
                {
                    Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                    var expected = new[]
                    {
                        typeof(SpaceEngineers.Core.GenericEndpoint.Implementations.AdvancedIntegrationContext)
                    };

                    var actual = advancedIntegrationContext
                        .FlattenDecoratedType()
                        .ShowTypes("extended integration context", log)
                        .ToList();

                    Assert.Equal(expected, actual);

                    var expectedPipeline = new[]
                    {
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.ErrorHandlingPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.UnitOfWorkPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Statistics.Internals.StatisticsPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.QueryReplyValidationPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.MessageHandlerPipeline),
                    };

                    var actualPipeline = endpointDependencyContainer
                        .Resolve<IMessagePipeline>()
                        .FlattenDecoratedType()
                        .ShowTypes("message pipeline", log)
                        .ToList();

                    Assert.Equal(expectedPipeline, actualPipeline);

                    var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                    var expectedIntegrationMessageTypes = new[]
                    {
                        typeof(IIntegrationMessage),
                        typeof(IIntegrationCommand),
                        typeof(IIntegrationEvent),
                        typeof(IIntegrationReply),
                        typeof(IIntegrationQuery<>),
                        typeof(BaseEvent),
                        typeof(InheritedEvent),
                        typeof(Command),
                        typeof(OpenGenericHandlerCommand),
                        typeof(Query),
                        typeof(Reply),
                        typeof(CaptureMessageStatistics),
                        typeof(GetEndpointStatistics),
                        typeof(EndpointStatisticsReply)
                    };

                    var actualIntegrationMessageTypes = integrationTypeProvider
                        .IntegrationMessageTypes()
                        .ShowTypes(nameof(IIntegrationTypeProvider.IntegrationMessageTypes), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedIntegrationMessageTypes.OrderBy(type => type.FullName).ToList(), actualIntegrationMessageTypes);

                    var expectedCommands = new[]
                    {
                        typeof(Command),
                        typeof(OpenGenericHandlerCommand)
                    };

                    var actualCommands = integrationTypeProvider
                        .EndpointCommands()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointCommands), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedCommands.OrderBy(type => type.FullName).ToList(), actualCommands);

                    var expectedQueries = new[]
                    {
                        typeof(Query)
                    };

                    var actualQueries = integrationTypeProvider
                        .EndpointQueries()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointQueries), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedQueries.OrderBy(type => type.FullName).ToList(), actualQueries);

                    var expectedReplies = new[]
                    {
                        typeof(Reply)
                    };

                    var actualReplies = integrationTypeProvider
                        .RepliesSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.RepliesSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedReplies.OrderBy(type => type.FullName).ToList(), actualReplies);

                    var expectedEvents = new[]
                    {
                        typeof(IIntegrationEvent),
                        typeof(BaseEvent),
                        typeof(InheritedEvent)
                    };

                    var actualEvents = integrationTypeProvider
                        .EventsSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EventsSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedEvents.OrderBy(type => type.FullName).ToList(), actualEvents);

                    var expectedCommandHandlers = new[]
                    {
                        typeof(CommandEmptyMessageHandler)
                    };

                    var actualCommandHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<Command>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualCommandHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedCommandHandlers.OrderBy(type => type.FullName).ToList(), actualCommandHandlers);

                    var expectedOpenGenericHandlerCommandHandlers = new[]
                    {
                        typeof(OpenGenericCommandEmptyMessageHandler<OpenGenericHandlerCommand>)
                    };

                    var actualOpenGenericHandlerCommandHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<OpenGenericHandlerCommand>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualOpenGenericHandlerCommandHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(actualOpenGenericHandlerCommandHandlers.OrderBy(type => type.FullName).ToList(), expectedOpenGenericHandlerCommandHandlers);

                    var expectedBaseEventHandlers = new[]
                    {
                        typeof(IntegrationEventEmptyMessageHandler),
                        typeof(BaseEventEmptyMessageHandler)
                    };

                    var actualBaseEventHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<BaseEvent>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualBaseEventHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedBaseEventHandlers.OrderBy(type => type.FullName).ToList(), actualBaseEventHandlers);

                    var expectedFirstInheritedEventHandlers = new[]
                    {
                        typeof(IntegrationEventEmptyMessageHandler),
                        typeof(BaseEventEmptyMessageHandler),
                        typeof(InheritedEventEmptyMessageHandler)
                    };

                    var actualFirstInheritedEventHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<InheritedEvent>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualInheritedEventHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedFirstInheritedEventHandlers.OrderBy(type => type.FullName).ToList(), actualFirstInheritedEventHandlers);

                    var expectedSecondInheritedEventHandlers = new[]
                    {
                        typeof(BaseEventEmptyMessageHandler)
                    };
                }
            }

            static void CheckTransport(IHost host, Action<string> log)
            {
                var transportDependencyContainer = host.GetTransportDependencyContainer();

                _ = transportDependencyContainer.Resolve<IIntegrationTransport>();
                var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                var expected = new[]
                {
                    typeof(SpaceEngineers.Core.IntegrationTransport.Implementations.IntegrationContext)
                };

                var actual = integrationContext
                    .FlattenDecoratedType()
                    .ShowTypes("transport integration context", log)
                    .ToList();

                Assert.Equal(expected, actual);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RunTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(new EndpointIdentity(nameof(RunTest), 0)))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var runningHost = host.RunAsync(cts.Token);
                await waitUntilTransportIsNotRunning.ConfigureAwait(false);
                await host.StopAsync(cts.Token).ConfigureAwait(false);
                await runningHost.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Empty(collector.Messages);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task StartStopTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(new EndpointIdentity(nameof(StartStopTest), 0)))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);
                await waitUntilTransportIsNotRunning.ConfigureAwait(false);
                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Empty(collector.Messages);
            }
        }

        private static async Task WaitUntilTransportIsNotRunning(IHost host, Action<string> log)
        {
            var tcs = new TaskCompletionSource();
            var subscription = MakeSubscription(tcs, log);

            var integrationTransport = host
                .GetTransportDependencyContainer()
                .Resolve<IIntegrationTransport>();

            using (Disposable.Create((integrationTransport, subscription), Subscribe, Unsubscribe))
            {
                log("Wait until transport is not started");
                await tcs.Task.ConfigureAwait(false);
            }

            static EventHandler<IntegrationTransportStatusChangedEventArgs> MakeSubscription(TaskCompletionSource tcs, Action<string> log)
            {
                return (s, e) =>
                {
                    log($"{s.GetType().Name}: {e.PreviousStatus} -> {e.CurrentStatus}");

                    if (e.CurrentStatus == EnIntegrationTransportStatus.Running)
                    {
                        tcs.TrySetResult();
                    }
                };
            }

            static void Subscribe((IIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged += subscription;
            }

            static void Unsubscribe((IIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged -= subscription;
            }
        }
    }
}