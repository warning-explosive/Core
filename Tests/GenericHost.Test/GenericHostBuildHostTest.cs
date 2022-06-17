﻿namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Api.Abstractions;
    using DataAccess.Orm.Host;
    using DataAccess.Orm.PostgreSql.Host;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.StartupActions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericEndpoint.Pipeline;
    using GenericEndpoint.Tracing.Pipeline;
    using GenericHost;
    using IntegrationTransport.Host;
    using IntegrationTransport.Host.BackgroundWorkers;
    using IntegrationTransport.RpcRequest;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Overrides;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
    using SpaceEngineers.Core.CompositionRoot.Api.Exceptions;
    using SpaceEngineers.Core.CompositionRoot.Api.Extensions;
    using SpaceEngineers.Core.DataAccess.Orm.Host.Migrations;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;
    using SpaceEngineers.Core.GenericEndpoint.DataAccess.BackgroundWorkers;
    using SpaceEngineers.Core.GenericEndpoint.DataAccess.StartupActions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using TracingEndpoint.Contract.Messages;
    using Xunit;
    using Xunit.Abstractions;
    using IIntegrationContext = IntegrationTransport.Api.Abstractions.IIntegrationContext;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class GenericHostBuildHostTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public GenericHostBuildHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// useContainer; useTransport;
        /// </summary>
        /// <returns>BuildHostTestData</returns>
        public static IEnumerable<object[]> BuildHostTestData()
        {
            var useInMemoryIntegrationTransport = new Func<ILogger, IHostBuilder, IHostBuilder>(
                (logger, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(logger)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<ILogger, IHostBuilder, IHostBuilder>(
                (logger, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(logger)))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                /*TODO: #180 - useRabbitMqIntegrationTransport*/
            };

            return integrationTransportProviders
               .Select(useTransport => new object[]
               {
                   useTransport
               });
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void SameTransportTest(Func<ILogger, IHostBuilder, IHostBuilder> useTransport)
        {
            var logger = Fixture.CreateLogger(Output);

            var host = useTransport(logger, Fixture.CreateHostBuilder(Output))
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithTracing()
                        .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(logger)))
                        .BuildOptions())
                .UseEndpoint(
                    TestIdentity.Endpoint20,
                    (_, builder) => builder
                        .WithTracing()
                        .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(logger)))
                        .BuildOptions())
                .BuildHost();

            var integrationTransport = host.GetTransportDependencyContainer().Resolve<IIntegrationTransport>();
            Output.WriteLine($"{nameof(IIntegrationTransport)}: {integrationTransport.GetType().FullName}");

            var transportIsSame = new[]
                {
                    host.GetEndpointDependencyContainer(TestIdentity.Endpoint10),
                    host.GetEndpointDependencyContainer(TestIdentity.Endpoint20)
                }
                .Select(container => container.Resolve<IIntegrationTransport>())
                .All(endpointTransport => ReferenceEquals(integrationTransport, endpointTransport));

            Assert.True(transportIsSame);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void BuildHostTest(Func<ILogger, IHostBuilder, IHostBuilder> useTransport)
        {
            var logger = Fixture.CreateLogger(Output);

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
                typeof(BaseEventEmptyMessageHandler),
                typeof(InheritedEventEmptyMessageHandler),
                typeof(CommandEmptyMessageHandler),
                typeof(OpenGenericCommandEmptyMessageHandler<>),
                typeof(QueryAlwaysReplyMessageHandler),
                typeof(ReplyEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var databaseProvider = new PostgreSqlDatabaseProvider();

            var settingsScope = nameof(BuildHostTest);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(logger, Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(databaseProvider)
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithOverrides(overrides)
                           .WithAdditionalOurTypes(additionalOurTypes))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            using (host)
            {
                CheckHost(host);
                CheckEndpoint(host, TestIdentity.Endpoint10, Output.WriteLine);
                CheckTransport(host, Output.WriteLine);
            }

            static void CheckHost(IHost host)
            {
                _ = host.Services.GetRequiredService<IHostedService>();

                var expectedHostStartupActions = new[]
                {
                    typeof(UpgradeDatabaseHostStartupAction),
                    typeof(GenericEndpointInboxHostStartupAction),
                    typeof(GenericEndpointHostStartupAction)
                };

                var actualHostStartupActions = host
                    .Services
                    .GetServices<IHostStartupAction>()
                    .Select(startup => startup.GetType())
                    .OrderBy(type => type.FullName)
                    .ToList();

                Assert.Equal(expectedHostStartupActions.OrderBy(type => type.FullName).ToList(), actualHostStartupActions);

                var expectedHostBackgroundWorkers = new[]
                {
                    typeof(GenericEndpointOutboxHostBackgroundWorker),
                    typeof(IntegrationTransportHostBackgroundWorker)
                };

                var actualHostBackgroundWorkers = host
                    .Services
                    .GetServices<IHostBackgroundWorker>()
                    .Select(startup => startup.GetType())
                    .OrderBy(type => type.FullName)
                    .ToList();

                Assert.Equal(expectedHostBackgroundWorkers.OrderBy(type => type.FullName).ToList(), actualHostBackgroundWorkers);
            }

            static void CheckEndpoint(IHost host, EndpointIdentity endpointIdentity, Action<string> log)
            {
                var endpointDependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);
                var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command));

                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<Core.GenericEndpoint.Api.Abstractions.IIntegrationContext>());

                using (endpointDependencyContainer.OpenScope())
                {
                    Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                    var expectedContexts = new[]
                    {
                        typeof(AdvancedIntegrationContext)
                    };

                    var actualContexts = advancedIntegrationContext
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("extended integration context", log)
                        .ToList();

                    Assert.Equal(expectedContexts, actualContexts);

                    var expectedPipeline = new[]
                    {
                        typeof(ErrorHandlingPipeline),
                        typeof(TracingPipeline),
                        typeof(UnitOfWorkPipeline),
                        typeof(QueryReplyValidationPipeline),
                        typeof(HandledByEndpointPipeline),
                        typeof(MessageHandlerPipeline)
                    };

                    var actualPipeline = endpointDependencyContainer
                        .Resolve<IMessagePipeline>()
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("message pipeline", log)
                        .ToList();

                    Assert.Equal(expectedPipeline, actualPipeline);

                    var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                    var expectedIntegrationMessageTypes = new[]
                    {
                        typeof(BaseEvent),
                        typeof(InheritedEvent),
                        typeof(Command),
                        typeof(OpenGenericHandlerCommand),
                        typeof(Query),
                        typeof(Reply),
                        typeof(CaptureTrace),
                        typeof(GetConversationTrace),
                        typeof(ConversationTrace)
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
                        typeof(BaseEvent),
                        typeof(InheritedEvent)
                    };

                    var actualEvents = integrationTypeProvider
                        .EventsSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EventsSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedEvents.OrderBy(type => type.FullName).ToList(), actualEvents);

                    Assert.Equal(typeof(BaseEventEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<BaseEvent>>().GetType());
                    Assert.Equal(typeof(InheritedEventEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<InheritedEvent>>().GetType());
                    Assert.Equal(typeof(CommandEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Command>>().GetType());
                    Assert.Equal(typeof(OpenGenericCommandEmptyMessageHandler<OpenGenericHandlerCommand>), endpointDependencyContainer.Resolve<IMessageHandler<OpenGenericHandlerCommand>>().GetType());
                    Assert.Equal(typeof(QueryAlwaysReplyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Query>>().GetType());
                    Assert.Equal(typeof(ReplyEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Reply>>().GetType());

                    var expectedErrorHandlers = new[]
                    {
                        typeof(TracingErrorHandler),
                        typeof(RetryErrorHandler)
                    };

                    var actualErrorHandlers = endpointDependencyContainer
                       .ResolveCollection<IErrorHandler>()
                       .Select(obj => obj.GetType())
                       .ShowTypes(nameof(IErrorHandler), log)
                       .ToList();

                    Assert.Equal(expectedErrorHandlers, actualErrorHandlers);
                }
            }

            static void CheckTransport(IHost host, Action<string> log)
            {
                var transportDependencyContainer = host.GetTransportDependencyContainer();

                var integrationTransport = transportDependencyContainer.Resolve<IIntegrationTransport>();
                log($"{nameof(IIntegrationTransport)}: {integrationTransport.GetType().FullName}");

                using (transportDependencyContainer.OpenScope())
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var expectedContexts = new[]
                    {
                        typeof(IntegrationTransport.Integration.IntegrationContext)
                    };

                    var actualContexts = integrationContext
                       .FlattenDecoratedObject(obj => obj.GetType())
                       .ShowTypes("transport integration context", log)
                       .ToList();

                    Assert.Equal(expectedContexts, actualContexts);
                }

                _ = transportDependencyContainer.Resolve<IRpcRequestRegistry>();

                using (transportDependencyContainer.OpenScope())
                {
                    var expectedRpcReplyHandlers = new[]
                    {
                        typeof(IntegrationTransport.RpcRequest.ErrorHandlingRpcReplyMessageHandler<IIntegrationReply>),
                        typeof(IntegrationTransport.Tracing.RpcRequest.TracingRpcReplyMessageHandler<IIntegrationReply>),
                        typeof(IntegrationTransport.RpcRequest.RpcReplyMessageHandler<IIntegrationReply>)
                    };

                    var actualRpcReplyHandlers = transportDependencyContainer
                        .Resolve<IRpcReplyMessageHandler<IIntegrationReply>>()
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("rpc reply handlers", log)
                        .ToList();

                    Assert.Equal(expectedRpcReplyHandlers, actualRpcReplyHandlers);
                }
            }
        }
    }
}