namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.PostgreSql.Host;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.TestExtensions;
    using GenericHost;
    using GenericHost.Api.Abstractions;
    using InMemoryIntegrationTransport.Host;
    using InMemoryIntegrationTransport.Host.Internals;
    using IntegrationTransport.Api.Abstractions;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using StatisticsEndpoint.Contract;
    using StatisticsEndpoint.Contract.Messages;
    using StatisticsEndpoint.Host;
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

        /// <summary> TransportTestData member </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> TransportTestData()
        {
            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder.UseInMemoryIntegrationTransport());

            yield return new object[] { useInMemoryIntegrationTransport };
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task RpcRequestTest(Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var statisticsEndpointIdentity = new EndpointIdentity(StatisticsEndpointIdentity.LogicalName, 0);

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
                .UseStatisticsEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithOrm(new PostgreSqlDatabaseProvider())
                    .BuildOptions(statisticsEndpointIdentity))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var query1 = new GetEndpointStatistics(statisticsEndpointIdentity);

                var reply1 = await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .RpcRequest<GetEndpointStatistics, EndpointStatisticsReply>(query1, cts.Token)
                    .ConfigureAwait(false);

                Assert.Equal(statisticsEndpointIdentity, reply1.EndpointIdentity);

                var query2 = new IdentifiedQuery(42);

                var reply2 = await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .RpcRequest<IdentifiedQuery, IdentifiedReply>(query2, cts.Token)
                    .ConfigureAwait(false);

                Assert.Equal(query2.Id, reply2.Id);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task EndpointCanHaveSeveralMessageHandlersPerMessage(Func<IHostBuilder, IHostBuilder> useTransport)
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
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await Task.Delay(3, cts.Token).ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task VariantMessageHandlerTest(Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var actualMessagesCount = 0;
            var expectedMessagesCount = 3;

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
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var integrationContext = host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>();

                await integrationContext.Publish(new BaseEvent(), cts.Token).ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            Output.WriteLine($"{nameof(actualMessagesCount)}: {actualMessagesCount}");
            Assert.Equal(expectedMessagesCount, actualMessagesCount);
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task ThrowingMessageHandlerTest(Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var actualIncomingMessagesCount = 0;
            var actualRefusedMessagesCount = 0;
            var incomingMessages = new ConcurrentBag<IntegrationMessage>();
            var failedMessages = new ConcurrentBag<(IntegrationMessage message, Exception exception)>();

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

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(additionalOurTypes))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var integrationContext = host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>();

                await integrationContext.Send(new IdentifiedCommand(42), cts.Token).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(4), cts.Token).ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

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
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task SimpleHostTest(Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var expectedMessagesCount = 1000;
            var expectedRefusedMessagesCount = 0;

            var actualMessagesCount = 0;
            var actualRefusedMessagesCount = 0;

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
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(endpoint1AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(endpoint1AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint11))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(endpoint2AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint20))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var integrationContext = host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>();

                await SendInitiationMessages(integrationContext, expectedMessagesCount, cts.Token).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(3), cts.Token).ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

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
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task SameTransportTest(Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(TestIdentity.Endpoint20))
                .BuildHost();

            bool transportIsSame;

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var transport = host.GetTransportDependencyContainer().Resolve<IIntegrationTransport>();

                transportIsSame = new[]
                    {
                        host.GetEndpointDependencyContainer(TestIdentity.Endpoint10),
                        host.GetEndpointDependencyContainer(TestIdentity.Endpoint20)
                    }
                    .Select(container => container.Resolve<IIntegrationTransport>())
                    .All(endpointTransport => ReferenceEquals(transport, endpointTransport));

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            Assert.True(transportIsSame);
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal void MessageHandlerTestExtensionsTest(Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var endpointIdentity = new EndpointIdentity(nameof(MessageHandlerTestExtensionsTest), 0);

            var messageTypes = new[]
            {
                typeof(IdentifiedCommand),
                typeof(IdentifiedEvent),
                typeof(IdentifiedQuery),
                typeof(IdentifiedReply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(IdentifiedCommandEmptyMessageHandler),
                typeof(IdentifiedCommandThrowingMessageHandler),
                typeof(IdentifiedEventEmptyMessageHandler),
                typeof(IdentifiedQueryOddReplyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(additionalOurTypes))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            var dependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);

            dependencyContainer
                .Resolve<IdentifiedCommandEmptyMessageHandler>()
                .OnMessage(new IdentifiedCommand(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            dependencyContainer
                .Resolve<IdentifiedCommandThrowingMessageHandler>()
                .OnMessage(new IdentifiedCommand(42))
                .ProducesNothing()
                .Throws<InvalidOperationException>(ex => ex.Message == "42")
                .Invoke();

            dependencyContainer
                .Resolve<IdentifiedEventEmptyMessageHandler>()
                .OnMessage(new IdentifiedEvent(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            dependencyContainer
                .Resolve<IdentifiedQueryOddReplyMessageHandler>()
                .OnMessage(new IdentifiedQuery(42))
                .ProducesNothing()
                .DoesNotThrow()
                .Invoke();

            dependencyContainer
                .Resolve<IdentifiedQueryOddReplyMessageHandler>()
                .OnMessage(new IdentifiedQuery(43))
                .DoesNotSend<IIntegrationCommand>()
                .DoesNotPublish<IIntegrationEvent>()
                .DoesNotRequest<IdentifiedQuery, IdentifiedReply>()
                .Replies<IdentifiedReply>(reply => reply.Id == 43)
                .DoesNotThrow()
                .Invoke();
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal void BuildHostTest(Func<IHostBuilder, IHostBuilder> useTransport)
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

            var testHost = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .ModifyContainerOptions(ExtendedTypeProviderDecorator.ExtendTypeProvider(additionalOurTypes))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            using (testHost)
            {
                CheckHost(testHost);
                CheckEndpoint(testHost);
                CheckTransport(testHost);
            }

            void CheckHost(IHost host)
            {
                _ = host.Services.GetRequiredService<IHostedService>();

                var hostStartupActions = host
                    .Services
                    .GetServices<IHostStartupAction>()
                    .ToList();

                Assert.Single(hostStartupActions);
                var hostStartupAction = hostStartupActions.Single();

                Assert.Equal(typeof(SpaceEngineers.Core.GenericEndpoint.Host.Internals.GenericEndpointHostStartupAction), hostStartupAction.GetType());

                var hostBackgroundWorkers = host
                    .Services
                    .GetServices<IHostBackgroundWorker>()
                    .ToList();

                Assert.Single(hostBackgroundWorkers);
                var hostBackgroundWorker = hostBackgroundWorkers.Single();

                Assert.Equal(typeof(IntegrationTransportHostBackgroundWorker), hostBackgroundWorker.GetType());
            }

            void CheckEndpoint(IHost host)
            {
                var endpointDependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);
                var integrationMessage = new IntegrationMessage(new IdentifiedCommand(0), typeof(IdentifiedCommand), new StringFormatterMock());

                Assert.Throws<SimpleInjector.ActivationException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                Assert.Throws<SimpleInjector.ActivationException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                Assert.Throws<SimpleInjector.ActivationException>(() => endpointDependencyContainer.Resolve<Core.GenericEndpoint.Api.Abstractions.IIntegrationContext>());

                using (endpointDependencyContainer.OpenScope())
                {
                    Assert.Throws<SimpleInjector.ActivationException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                    var expected = new[]
                    {
                        typeof(SpaceEngineers.Core.GenericEndpoint.Internals.AdvancedIntegrationContext)
                    };

                    var actual = advancedIntegrationContext
                        .UnwrapDecorators()
                        .ShowTypes("#extended integration context", Output.WriteLine)
                        .ToList();

                    Assert.Equal(expected, actual);

                    var expectedPipeline = new[]
                    {
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.ErrorHandlingPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Statistics.Internals.StatisticsPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.UnitOfWorkPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.QueryReplyValidationPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.MessageHandlerPipeline),
                    };

                    var actualPipeline = endpointDependencyContainer
                        .Resolve<IMessagePipeline>()
                        .UnwrapDecorators()
                        .ShowTypes("#message pipeline", Output.WriteLine)
                        .ToList();

                    Assert.Equal(expectedPipeline, actualPipeline);

                    var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                    var expectedIntegrationMessageTypes = new[]
                    {
                        typeof(IIntegrationMessage),
                        typeof(IIntegrationCommand),
                        typeof(IIntegrationEvent),
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
                        .ShowTypes(nameof(IIntegrationTypeProvider.IntegrationMessageTypes), Output.WriteLine)
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
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointCommands), Output.WriteLine)
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
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointEvents), Output.WriteLine)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedEvents.OrderBy(type => type.FullName).ToList(), actualEvents);

                    var expectedQueries = new[]
                    {
                        typeof(IIntegrationQuery<>),
                        typeof(IdentifiedQuery)
                    };

                    var actualQueries = integrationTypeProvider
                        .EndpointQueries()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointQueries), Output.WriteLine)
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
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointSubscriptions), Output.WriteLine)
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
                        .ShowTypes("actualBaseEventHandlers", Output.WriteLine)
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
                        .ShowTypes("actualFirstInheritedEventHandlers", Output.WriteLine)
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
                        .ShowTypes("actualSecondInheritedEventHandlers", Output.WriteLine)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedSecondInheritedEventHandlers.OrderBy(type => type.FullName).ToList(), actualSecondInheritedEventHandlers);
                }
            }

            void CheckTransport(IHost host)
            {
                var transportDependencyContainer = host.GetTransportDependencyContainer();

                _ = transportDependencyContainer.Resolve<IIntegrationTransport>();
                var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                var expected = new[]
                {
                    typeof(SpaceEngineers.Core.IntegrationTransport.Internals.IntegrationContext)
                };

                var actual = integrationContext
                    .UnwrapDecorators()
                    .ShowTypes("#transport integration context", Output.WriteLine)
                    .ToList();

                Assert.Equal(expected, actual);
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task RunTest(Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(new EndpointIdentity(nameof(RunTest), 0)))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.RunAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task StartStopTest(Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(new EndpointIdentity(nameof(StartStopTest), 0)))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(3), cts.Token).ConfigureAwait(false);
                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }
    }
}