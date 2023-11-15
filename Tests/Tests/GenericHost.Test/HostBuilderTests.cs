namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Exceptions;
    using CompositionRoot.Extensions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Authorization;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.Contract;
    using GenericEndpoint.DataAccess.Sql.Host.BackgroundWorkers;
    using GenericEndpoint.DataAccess.Sql.Host.StartupActions;
    using GenericEndpoint.DataAccess.Sql.Postgres.Host;
    using GenericEndpoint.DataAccess.Sql.Postgres.Host.StartupActions;
    using GenericEndpoint.EventSourcing.Host;
    using GenericEndpoint.EventSourcing.Host.StartupActions;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.StartupActions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Pipeline;
    using GenericEndpoint.Telemetry;
    using GenericEndpoint.Telemetry.Host;
    using GenericEndpoint.Web.Host;
    using IntegrationTransport.Api;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Host;
    using IntegrationTransport.Host.BackgroundWorkers;
    using IntegrationTransport.RabbitMQ;
    using IntegrationTransport.RabbitMQ.Settings;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Xunit;
    using Xunit.Abstractions;
    using Identity = IntegrationTransport.InMemory.Identity;
    using IntegrationMessage = GenericEndpoint.Messaging.IntegrationMessage;

    /// <summary>
    /// HostBuilder tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class HostBuilderTests : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public HostBuilderTests(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// Test cases for HostBuilder tests
        /// </summary>
        /// <returns>Test cases</returns>
        public static IEnumerable<object[]> HostBuilderTestData()
        {
            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory wasn't found");

            var settingsDirectory = projectFileDirectory
                .StepInto("Settings")
                .StepInto(nameof(HostBuilderTests));

            var inMemoryIntegrationTransportIdentity = Identity.TransportIdentity();

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, TransportIdentity, IHostBuilder>(
                static (hostBuilder, transportIdentity) => hostBuilder.UseInMemoryIntegrationTransport(transportIdentity));

            var rabbitMqIntegrationTransportIdentity = IntegrationTransport.RabbitMQ.Identity.TransportIdentity();

            var useRabbitMqIntegrationTransport = new Func<IHostBuilder, TransportIdentity, IHostBuilder>(
                static (hostBuilder, transportIdentity) => hostBuilder.UseRabbitMqIntegrationTransport(transportIdentity));

            var integrationTransportProviders = new[]
            {
                (inMemoryIntegrationTransportIdentity, useInMemoryIntegrationTransport),
                (rabbitMqIntegrationTransportIdentity, useRabbitMqIntegrationTransport)
            };

            return integrationTransportProviders
               .Select(transport =>
               {
                   var (transportIdentity, useTransport) = transport;

                   return new object[]
                   {
                       settingsDirectory,
                       transportIdentity,
                       useTransport
                   };
               });
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(HostBuilderTestData))]
        internal void HostBuilder_utilizes_same_integration_transport_instance_between_its_endpoints(
            DirectoryInfo settingsDirectory,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    builder => builder.BuildOptions())
                .UseEndpoint(
                    TestIdentity.Endpoint20,
                    builder => builder.BuildOptions())
                .BuildHost(settingsDirectory);

            var gatewayHost = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .BuildHost(settingsDirectory);

            var gatewayTransport = gatewayHost.GetIntegrationTransportDependencyContainer(transportIdentity).Resolve<IIntegrationTransport>();
            var hostTransport = host.GetIntegrationTransportDependencyContainer(transportIdentity).Resolve<IIntegrationTransport>();
            var endpoint10Transport = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10).Resolve<IIntegrationTransport>();
            var endpoint20Transport = host.GetEndpointDependencyContainer(TestIdentity.Endpoint20).Resolve<IIntegrationTransport>();

            Assert.NotSame(hostTransport, gatewayTransport);
            Assert.Same(hostTransport, endpoint10Transport);
            Assert.Same(hostTransport, endpoint20Transport);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(HostBuilderTestData))]
        internal void Assert_host_and_its_endpoints_configuration(
            DirectoryInfo settingsDirectory,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var messageTypes = new[]
            {
                typeof(BaseEvent),
                typeof(InheritedEvent),
                typeof(Command),
                typeof(OpenGenericHandlerCommand),
                typeof(Request),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(BaseEventHandler),
                typeof(InheritedEventHandler),
                typeof(CommandHandler),
                typeof(OpenGenericCommandHandler<>),
                typeof(AlwaysReplyRequestHandler),
                typeof(ReplyHandler)
            };

            var additionalOurTypes = messageTypes
               .Concat(messageHandlerTypes)
               .ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => builder
                        .WithPostgreSqlDataAccess(options => options
                            .ExecuteMigrations())
                        .WithSqlEventSourcing()
                        .WithJwtAuthentication(builder.Context.Configuration)
                        .WithAuthorization()
                        .WithOpenTelemetry()
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .UseOpenTelemetry()
                .BuildHost(settingsDirectory);

            using (host)
            {
                CheckHost(host);
                CheckTransport(host, transportIdentity, Output.WriteLine);
                CheckEndpoint(host, TestIdentity.Endpoint10, Output.WriteLine);
            }

            static void CheckHost(IHost host)
            {
                var hostedServices = host
                    .Services
                    .GetServices<IHostedService>()
                    .OfType<HostedService>()
                    .ToArray();

                Assert.Equal(2, hostedServices.Length);
            }

            static void CheckTransport(IHost host, TransportIdentity transportIdentity, Action<string> log)
            {
                log($"Endpoint: {transportIdentity}");

                IDependencyContainer integrationTransportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);

                // IHostedServiceStartupAction
                {
                    var expectedHostedServiceStartupActions = Array
                        .Empty<Type>()
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    var actualHostedServiceStartupActions = integrationTransportDependencyContainer
                        .ResolveCollection<IHostedServiceStartupAction>()
                        .Select(startup => startup.GetType())
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedHostedServiceStartupActions, actualHostedServiceStartupActions);
                }

                // IHostedServiceBackgroundWorker
                {
                    var expectedHostedServiceBackgroundWorkers = new[]
                        {
                            typeof(IntegrationTransportHostedServiceBackgroundWorker)
                        }
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    var actualHostedServiceBackgroundWorkers = integrationTransportDependencyContainer
                        .ResolveCollection<IHostedServiceBackgroundWorker>()
                        .Select(startup => startup.GetType())
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedHostedServiceBackgroundWorkers, actualHostedServiceBackgroundWorkers);
                }

                // IHostedServiceObject
                {
                    var expectedHostedServiceBackgroundWorkers = new[]
                        {
                            typeof(IntegrationTransportHostedServiceBackgroundWorker)
                        }
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    var actualHostedServiceObjects = integrationTransportDependencyContainer
                        .ResolveCollection<IHostedServiceObject>()
                        .Select(startup => startup.GetType())
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedHostedServiceBackgroundWorkers, actualHostedServiceObjects);
                }

                // IntegrationContext
                {
                    var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command));

                    Assert.Throws<ComponentResolutionException>(() => integrationTransportDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    Assert.Throws<ComponentResolutionException>(() => integrationTransportDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                    Assert.Throws<ComponentResolutionException>(() => integrationTransportDependencyContainer.Resolve<IIntegrationContext>());
                }
            }

            static void CheckEndpoint(IHost host, EndpointIdentity endpointIdentity, Action<string> log)
            {
                log($"Endpoint: {endpointIdentity}");

                var endpointDependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);

                // IHostedServiceStartupAction
                {
                    var expectedHostedServiceStartupActions = new[]
                        {
                            typeof(EventSourcingHostedServiceStartupAction),
                            typeof(InboxInvalidationHostedServiceStartupAction),
                            typeof(MessagingHostedServiceStartupAction),
                            typeof(UpgradeDatabaseHostedServiceStartupAction),
                            typeof(ReloadNpgsqlTypesHostedServiceStartupAction),
                            typeof(GenericEndpointHostedServiceStartupAction)
                        }
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    var actualHostedServiceStartupActions = endpointDependencyContainer
                        .ResolveCollection<IHostedServiceStartupAction>()
                        .Select(startup => startup.GetType())
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedHostedServiceStartupActions, actualHostedServiceStartupActions);
                }

                // IHostedServiceBackgroundWorker
                {
                    var expectedHostedServiceBackgroundWorkers = new[]
                        {
                            typeof(GenericEndpointDataAccessHostedServiceBackgroundWorker)
                        }
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    var actualHostedServiceBackgroundWorkers = endpointDependencyContainer
                        .ResolveCollection<IHostedServiceBackgroundWorker>()
                        .Select(startup => startup.GetType())
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedHostedServiceBackgroundWorkers, actualHostedServiceBackgroundWorkers);
                }

                // IHostedServiceObject
                {
                    var expectedHostedServiceBackgroundWorkers = new[]
                        {
                            typeof(EventSourcingHostedServiceStartupAction),
                            typeof(InboxInvalidationHostedServiceStartupAction),
                            typeof(MessagingHostedServiceStartupAction),
                            typeof(UpgradeDatabaseHostedServiceStartupAction),
                            typeof(ReloadNpgsqlTypesHostedServiceStartupAction),
                            typeof(GenericEndpointHostedServiceStartupAction),
                            typeof(GenericEndpointDataAccessHostedServiceBackgroundWorker)
                        }
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    var actualHostedServiceObjects = endpointDependencyContainer
                        .ResolveCollection<IHostedServiceObject>()
                        .Select(startup => startup.GetType())
                        .OrderByDependencies()
                        .ThenBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedHostedServiceBackgroundWorkers, actualHostedServiceObjects);
                }

                // IntegrationContext
                {
                    var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command));

                    Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                    Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IIntegrationContext>());
                }

                using (endpointDependencyContainer.OpenScope())
                {
                    // IIntegrationContext
                    {
                        var expectedContexts = new[]
                        {
                            typeof(AdvancedIntegrationContext)
                        };

                        var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command));

                        Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                        var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                        var actualAdvancedIntegrationContexts = advancedIntegrationContext
                            .FlattenDecoratedObject(obj => obj.GetType())
                            .ShowTypes(nameof(IAdvancedIntegrationContext), log)
                            .ToList();

                        Assert.Equal(expectedContexts, actualAdvancedIntegrationContexts);

                        var integrationContext = endpointDependencyContainer.Resolve<IIntegrationContext>();

                        var actualIntegrationContexts = integrationContext
                            .FlattenDecoratedObject(obj => obj.GetType())
                            .ShowTypes(nameof(IIntegrationContext), log)
                            .ToList();

                        Assert.Equal(expectedContexts, actualIntegrationContexts);
                    }

                    // IMessagesCollector
                    {
                        var expectedMessagesCollector = new[]
                        {
                            typeof(MessagesCollector)
                        };

                        var messagesCollector = endpointDependencyContainer.Resolve<IMessagesCollector>();

                        var actualMessagesCollector = messagesCollector
                            .FlattenDecoratedObject(obj => obj.GetType())
                            .ShowTypes(nameof(IMessagesCollector), log)
                            .ToList();

                        Assert.Equal(expectedMessagesCollector, actualMessagesCollector);
                    }

                    // IMessageHandlerMiddleware
                    {
                        var expectedMiddlewares = new[]
                        {
                            typeof(TracingMiddleware),
                            typeof(ErrorHandlingMiddleware),
                            typeof(AuthorizationMiddleware),
                            typeof(UnitOfWorkMiddleware),
                            typeof(HandledByEndpointMiddleware),
                            typeof(RequestReplyMiddleware)
                        };

                        var actualMiddlewares = endpointDependencyContainer
                            .ResolveCollection<IMessageHandlerMiddleware>()
                            .Select(middleware => middleware.GetType())
                            .ShowTypes(nameof(IMessageHandlerMiddleware), log)
                            .ToList();

                        Assert.Equal(expectedMiddlewares, actualMiddlewares);
                    }

                    // IIntegrationMessage
                    {
                        var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                        var expectedIntegrationMessageTypes = new[]
                            {
                                typeof(BaseEvent),
                                typeof(InheritedEvent),
                                typeof(Command),
                                typeof(OpenGenericHandlerCommand),
                                typeof(Request),
                                typeof(Reply)
                            }
                            .OrderBy(type => type.Name)
                            .ToList();

                        var actualIntegrationMessageTypes = integrationTypeProvider
                            .IntegrationMessageTypes()
                            .ShowTypes(nameof(IIntegrationTypeProvider.IntegrationMessageTypes), log)
                            .OrderBy(type => type.Name)
                            .ToList();

                        Assert.Equal(expectedIntegrationMessageTypes, actualIntegrationMessageTypes);
                    }

                    // IIntegrationCommand
                    {
                        var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                        var expectedCommands = new[]
                            {
                                typeof(Command),
                                typeof(OpenGenericHandlerCommand)
                            }
                            .OrderBy(type => type.Name)
                            .ToList();

                        var actualCommands = integrationTypeProvider
                            .EndpointCommands()
                            .ShowTypes(nameof(IIntegrationTypeProvider.EndpointCommands), log)
                            .OrderBy(type => type.Name)
                            .ToList();

                        Assert.Equal(expectedCommands, actualCommands);
                    }

                    // IIntegrationRequest
                    {
                        var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                        var expectedRequests = new[]
                            {
                                typeof(Request)
                            }
                            .OrderBy(type => type.Name)
                            .ToList();

                        var actualRequests = integrationTypeProvider
                            .EndpointRequests()
                            .ShowTypes(nameof(IIntegrationTypeProvider.EndpointRequests), log)
                            .OrderBy(type => type.Name)
                            .ToList();

                        Assert.Equal(expectedRequests, actualRequests);
                    }

                    // IIntegrationReply
                    {
                        var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                        var expectedReplies = new[]
                            {
                                typeof(Reply)
                            }
                            .OrderBy(type => type.Name)
                            .ToList();

                        var actualReplies = integrationTypeProvider
                            .RepliesSubscriptions()
                            .ShowTypes(nameof(IIntegrationTypeProvider.RepliesSubscriptions), log)
                            .OrderBy(type => type.Name)
                            .ToList();

                        Assert.Equal(expectedReplies, actualReplies);
                    }

                    // IIntegrationEvent
                    {
                        var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                        var expectedEvents = new[]
                            {
                                typeof(BaseEvent),
                                typeof(InheritedEvent)
                            }
                            .OrderBy(type => type.Name)
                            .ToList();

                        var actualEvents = integrationTypeProvider
                            .EventsSubscriptions()
                            .ShowTypes(nameof(IIntegrationTypeProvider.EventsSubscriptions), log)
                            .OrderBy(type => type.Name)
                            .ToList();

                        Assert.Equal(expectedEvents, actualEvents);
                    }

                    // IIntegrationMessageHeaderProvider
                    {
                        var expectedIntegrationMessageHeaderProvider = new[]
                            {
                                typeof(ConversationIdProvider),
                                typeof(MessageInitiatorProvider),
                                typeof(MessageOriginProvider),
                                typeof(ReplyToProvider),
                                typeof(TraceContextPropagationProvider),
                                typeof(UserScopeProvider),
                                typeof(AuthorizationHeaderProvider),
                                typeof(AnonymousUserScopeProvider)
                            }
                            .ToList();

                        var actualIntegrationMessageHeaderProvider = endpointDependencyContainer
                            .ResolveCollection<IIntegrationMessageHeaderProvider>()
                            .Select(obj => obj.GetType())
                            .ShowTypes(nameof(IIntegrationMessageHeaderProvider), log)
                            .ToList();

                        Assert.Equal(expectedIntegrationMessageHeaderProvider, actualIntegrationMessageHeaderProvider);
                    }

                    // IMessageHandler
                    {
                        Assert.Equal(typeof(BaseEventHandler), endpointDependencyContainer.Resolve<IMessageHandler<BaseEvent>>().GetType());
                        Assert.Equal(typeof(InheritedEventHandler), endpointDependencyContainer.Resolve<IMessageHandler<InheritedEvent>>().GetType());
                        Assert.Equal(typeof(CommandHandler), endpointDependencyContainer.Resolve<IMessageHandler<Command>>().GetType());
                        Assert.Equal(typeof(OpenGenericCommandHandler<OpenGenericHandlerCommand>), endpointDependencyContainer.Resolve<IMessageHandler<OpenGenericHandlerCommand>>().GetType());
                        Assert.Equal(typeof(AlwaysReplyRequestHandler), endpointDependencyContainer.Resolve<IMessageHandler<Request>>().GetType());
                        Assert.Equal(typeof(ReplyHandler), endpointDependencyContainer.Resolve<IMessageHandler<Reply>>().GetType());
                    }

                    // IErrorHandler
                    {
                        var expectedErrorHandlers = new[]
                        {
                            typeof(RetryErrorHandler),
                            typeof(TracingErrorHandler)
                        };

                        var actualErrorHandlers = endpointDependencyContainer
                            .ResolveCollection<IErrorHandler>()
                            .Select(obj => obj.GetType())
                            .ShowTypes(nameof(IErrorHandler), log)
                            .ToList();

                        Assert.Equal(expectedErrorHandlers, actualErrorHandlers);
                    }
                }
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(HostBuilderTestData))]
        internal void HostBuilder_throws_exception_if_there_are_endpoint_duplicates(
            DirectoryInfo settingsDirectory,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var endpointIdentity = TestIdentity.Endpoint10;

            InvalidOperationException? exception;

            try
            {
                _ = Fixture
                    .CreateHostBuilder()
                    .UseIntegrationTransport(transportIdentity, useTransport)
                    .UseEndpoint(endpointIdentity, builder => builder.BuildOptions())
                    .UseEndpoint(endpointIdentity, builder => builder.BuildOptions())
                    .BuildHost(settingsDirectory);

                exception = null;
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            Assert.NotNull(exception);
            Assert.Equal($"Endpoint duplicates was found: {endpointIdentity.LogicalName}. Horizontal scaling in the same process doesn't make sense.", exception.Message);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(HostBuilderTestData))]
        internal void HostBuilder_throws_exception_if_UseWebApiGateway_method_was_called_before_endpoint_declaration(
            DirectoryInfo settingsDirectory,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            InvalidOperationException? exception;

            try
            {
                _ = Fixture
                    .CreateHostBuilder()
                    .UseIntegrationTransport(transportIdentity, useTransport)
                    .UseWebApiGateway()
                    .UseEndpoint(TestIdentity.Endpoint10, builder => builder.BuildOptions())
                    .BuildHost(settingsDirectory);

                exception = null;
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            Assert.NotNull(exception);
            Assert.Equal(".UseWebApiGateway() should be called after all endpoint declarations", exception.Message);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(HostBuilderTestData))]
        internal void HostBuilder_throws_exception_if_UseIntegrationTransport_method_was_called_after_endpoint_declaration(
            DirectoryInfo settingsDirectory,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            InvalidOperationException? exception;

            try
            {
                _ = Fixture
                    .CreateHostBuilder()
                    .UseEndpoint(TestIdentity.Endpoint10, builder => builder.BuildOptions())
                    .UseIntegrationTransport(transportIdentity, useTransport)
                    .BuildHost(settingsDirectory);

                exception = null;
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            Assert.NotNull(exception);
            Assert.Equal(".UseIntegrationTransport() should be called before any endpoint declarations", exception.Message);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(HostBuilderTestData))]
        internal async Task Host_starts_and_stops_gracefully(
            DirectoryInfo settingsDirectory,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10, builder => builder.BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, StartStopTestInternal(settingsDirectory, transportIdentity))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> StartStopTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity)
            {
                return (_, host, _) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    return Task.CompletedTask;
                };
            }
        }
    }
}