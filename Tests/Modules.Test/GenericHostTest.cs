namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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

            var integrationTransportProviders = new object[]
            {
                new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder.UseInMemoryIntegrationTransport()),
            };

            return DependencyContainerTestData()
                .SelectMany(it => integrationTransportProviders.Select(provider => it.Concat(new[] { provider, timeout }).ToArray()));
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
        internal async Task RpcRequestTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(IdentifiedQuery),
                typeof(IdentifiedReply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(IdentifiedQueryAlwaysReplyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var query = new IdentifiedQuery(42);

                var reply = await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .RpcRequest<IdentifiedQuery, IdentifiedReply>(query, cts.Token)
                    .ConfigureAwait(false);

                Assert.Equal(query.Id, reply.Id);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EndpointCanHaveSeveralMessageHandlersPerMessage(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(IdentifiedCommand)
            };

            var messageHandlerTypes = new[]
            {
                typeof(IdentifiedCommandEmptyMessageHandler),
                typeof(IdentifiedCommandThrowingMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task VariantMessageHandlerTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            /* TODO: #112
            var actualMessagesCount = 0;
            var expectedMessagesCount = 3;*/

            var messageTypes = new[]
            {
                typeof(BaseEvent),
                typeof(FirstInheritedEvent),
                typeof(SecondInheritedEvent)
            };

            var messageHandlerTypes = new[]
            {
                typeof(BaseEventEmptyMessageHandler),
                typeof(FirstInheritedEventEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var integrationContext = host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>();

                await integrationContext.Publish(new BaseEvent(), cts.Token).ConfigureAwait(false);

                /* TODO: #112 - await until event is not handled */

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            /* TODO: #112
            Output.WriteLine($"{nameof(actualMessagesCount)}: {actualMessagesCount}");
            Assert.Equal(expectedMessagesCount, actualMessagesCount);*/
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ThrowingMessageHandlerTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            /* TODO: #112
            var actualIncomingMessagesCount = 0;
            var actualRefusedMessagesCount = 0;
            var incomingMessages = new ConcurrentBag<IntegrationMessage>();
            var failedMessages = new ConcurrentBag<(IntegrationMessage message, Exception exception)>();*/

            var endpointIdentity = new EndpointIdentity(TestIdentity.Endpoint1, 0);

            var messageTypes = new[]
            {
                typeof(IdentifiedCommand)
            };

            var messageHandlerTypes = new[]
            {
                typeof(IdentifiedCommandThrowingMessageHandler)
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
                    .WithStatistics()
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

                var integrationContext = host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>();

                await integrationContext.Send(new IdentifiedCommand(42), cts.Token).ConfigureAwait(false);

                /* TODO: #112 - await until all retry attempts is not proceed */

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            /* TODO: #112
            Output.WriteLine($"{nameof(actualIncomingMessagesCount)}: {actualIncomingMessagesCount}");
            Output.WriteLine(incomingMessages.Select((message, index) => $"[{index}] - {message}").ToString(Environment.NewLine));

            Assert.Equal(4, actualIncomingMessagesCount);
            Assert.Single(incomingMessages.Select(it => it.ReflectedType).Distinct());
            Assert.Single(incomingMessages.Select(it => it.Payload.ToString()).Distinct());
            Assert.Single(incomingMessages.Select(it => it.ReadRequiredHeader<Guid>(IntegrationMessageHeader.ConversationId)).Distinct());

            var actualRetryIndexes = incomingMessages
                .Select(it => it.ReadHeader<int>(IntegrationMessageHeader.RetryCounter))
                .OrderBy(it => it)
                .ToList();
            Assert.Equal(new List<int> { 0, 1, 2, 3 }, actualRetryIndexes);

            Output.WriteLine($"{nameof(actualRefusedMessagesCount)}: {actualRefusedMessagesCount}");
            Assert.Equal(1, actualRefusedMessagesCount);

            Assert.Single(failedMessages);
            var failedMessage = failedMessages.Single();
            Output.WriteLine(failedMessage.ToString());
            var exception = failedMessage.exception;
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("42", exception.Message);
            Assert.Equal("42", failedMessage.message.Payload.ToString());
            */
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task SimpleHostTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            /* TODO: #112
            var expectedMessagesCount = 1000;
            var expectedRefusedMessagesCount = 0;

            var actualMessagesCount = 0;
            var actualRefusedMessagesCount = 0;*/

            var messageTypes = new[]
            {
                typeof(IdentifiedCommand),
                typeof(IdentifiedEvent),
                typeof(IdentifiedQuery),
                typeof(IdentifiedReply)
            };

            var endpoint1MessageHandlerTypes = new[]
            {
                typeof(IdentifiedCommandEmptyMessageHandler),
                typeof(IdentifiedEventEmptyMessageHandler),
                typeof(IdentifiedQueryAlwaysReplyMessageHandler)
            };

            var endpoint2MessageHandlerTypes = new[]
            {
                typeof(IdentifiedEventEmptyMessageHandler)
            };

            var endpoint1AdditionalOurTypes = messageTypes.Concat(endpoint1MessageHandlerTypes).ToArray();
            var endpoint2AdditionalOurTypes = messageTypes.Concat(endpoint2MessageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint11))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(endpoint2AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint20))
                .BuildHost();

            var waitUntilTransportIsNotRunning = WaitUntilTransportIsNotRunning(host, Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var integrationContext = host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>();

                /* TODO: #112
                await SendInitiationMessages(integrationContext, expectedMessagesCount, cts.Token).ConfigureAwait(false);
                await until initiation message is not handled */

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            /* TODO: #112
            Output.WriteLine($"{nameof(actualMessagesCount)}: {actualMessagesCount}");
            Assert.Equal(expectedMessagesCount, actualMessagesCount);

            Output.WriteLine($"{nameof(actualRefusedMessagesCount)}: {actualRefusedMessagesCount}");
            Assert.Equal(expectedRefusedMessagesCount, actualRefusedMessagesCount);

            static async Task SendInitiationMessages(
                IIntegrationContext integrationContext,
                int count,
                CancellationToken token)
            {
                for (var i = 0; i < count; i++)
                {
                    var operation = i % 2 == 0
                        ? integrationContext.Send(new IdentifiedCommand(i), token)
                        : integrationContext.Publish(new IdentifiedEvent(i), token);

                    await operation.ConfigureAwait(false);
                }
            }*/
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
            new IdentifiedCommandEmptyMessageHandler()
                .OnMessage(new IdentifiedCommand(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            new IdentifiedCommandThrowingMessageHandler()
                .OnMessage(new IdentifiedCommand(42))
                .ProducesNothing()
                .Throws<InvalidOperationException>(ex => ex.Message == "42")
                .Invoke();

            new IdentifiedEventEmptyMessageHandler()
                .OnMessage(new IdentifiedEvent(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            new IdentifiedQueryOddReplyMessageHandler()
                .OnMessage(new IdentifiedQuery(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            new IdentifiedQueryOddReplyMessageHandler()
                .OnMessage(new IdentifiedQuery(43))
                .DoesNotSend<IIntegrationCommand>()
                .DoesNotPublish<IIntegrationEvent>()
                .DoesNotRequest<IdentifiedQuery, IdentifiedReply>()
                .Replies<IdentifiedReply>(reply => reply.Id == 43)
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
                typeof(FirstInheritedEvent),
                typeof(SecondInheritedEvent),
                typeof(IdentifiedCommand),
                typeof(IdentifiedEvent),
                typeof(IdentifiedQuery),
                typeof(IdentifiedReply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(BaseEventEmptyMessageHandler),
                typeof(FirstInheritedEventEmptyMessageHandler),
                typeof(IdentifiedCommandEmptyMessageHandler),
                typeof(IdentifiedEventEmptyMessageHandler),
                typeof(IdentifiedQueryAlwaysReplyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
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
                var integrationMessage = new IntegrationMessage(new IdentifiedCommand(0), typeof(IdentifiedCommand), new StringFormatterMock());

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
                        .ShowTypes("#extended integration context", log)
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
                        .ShowTypes("#message pipeline", log)
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
                        typeof(FirstInheritedEvent),
                        typeof(SecondInheritedEvent),
                        typeof(IdentifiedCommand),
                        typeof(IdentifiedEvent),
                        typeof(IdentifiedQuery),
                        typeof(IdentifiedReply),
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
                        typeof(IIntegrationCommand),
                        typeof(IdentifiedCommand)
                    };

                    var actualCommands = integrationTypeProvider
                        .EndpointCommands()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointCommands), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedCommands.OrderBy(type => type.FullName).ToList(), actualCommands);

                    var expectedEvents = new[]
                    {
                        typeof(IIntegrationEvent),
                        typeof(BaseEvent),
                        typeof(FirstInheritedEvent),
                        typeof(SecondInheritedEvent)
                    };

                    var actualEvents = integrationTypeProvider
                        .EndpointEvents()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointEvents), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedEvents.OrderBy(type => type.FullName).ToList(), actualEvents);

                    var expectedReplies = new[]
                    {
                        typeof(IIntegrationReply),
                        typeof(IdentifiedReply),
                        typeof(EndpointStatisticsReply)
                    };

                    var actualReplies = integrationTypeProvider
                        .Replies()
                        .ShowTypes(nameof(IIntegrationTypeProvider.Replies), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedReplies.OrderBy(type => type.FullName).ToList(), actualReplies);

                    var expectedQueries = new[]
                    {
                        typeof(IIntegrationQuery<>),
                        typeof(IdentifiedQuery)
                    };

                    var actualQueries = integrationTypeProvider
                        .EndpointQueries()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointQueries), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedQueries.OrderBy(type => type.FullName).ToList(), actualQueries);

                    var expectedSubscriptions = new[]
                    {
                        typeof(BaseEvent),
                        typeof(FirstInheritedEvent),
                        typeof(IdentifiedEvent)
                    };

                    var actualSubscriptions = integrationTypeProvider
                        .EndpointSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedSubscriptions.OrderBy(type => type.FullName).ToList(), actualSubscriptions);

                    var expectedBaseEventHandlers = new[]
                    {
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
                        typeof(BaseEventEmptyMessageHandler),
                        typeof(FirstInheritedEventEmptyMessageHandler)
                    };

                    var actualFirstInheritedEventHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<FirstInheritedEvent>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualFirstInheritedEventHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedFirstInheritedEventHandlers.OrderBy(type => type.FullName).ToList(), actualFirstInheritedEventHandlers);

                    var expectedSecondInheritedEventHandlers = new[]
                    {
                        typeof(BaseEventEmptyMessageHandler)
                    };

                    var actualSecondInheritedEventHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<SecondInheritedEvent>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualSecondInheritedEventHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedSecondInheritedEventHandlers.OrderBy(type => type.FullName).ToList(), actualSecondInheritedEventHandlers);
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
                    .ShowTypes("#transport integration context", log)
                    .ToList();

                Assert.Equal(expected, actual);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RunTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
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
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task StartStopTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
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