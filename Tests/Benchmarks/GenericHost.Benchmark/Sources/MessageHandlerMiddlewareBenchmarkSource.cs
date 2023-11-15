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
    using GenericEndpoint.Authorization;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.Contract;
    using GenericEndpoint.DataAccess.Sql.Postgres.Host;
    using GenericEndpoint.EventSourcing.Host;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using GenericEndpoint.Telemetry;
    using GenericEndpoint.Telemetry.Host;
    using IntegrationTransport.Host;
    using IntegrationTransport.RabbitMQ;
    using JwtAuthentication;
    using Test.Api.ClassFixtures;
    using IHost = Microsoft.Extensions.Hosting.IHost;

    /// <summary>
    /// IMessageHandlerMiddleware benchmark source
    /// </summary>
    [SimpleJob(RunStrategy.Throughput)]
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    [SuppressMessage("Analysis", "CA1001", Justification = "benchmark source")]
    public class MessageHandlerMiddlewareBenchmarkSource
    {
        private CancellationTokenSource? _cts;
        private IHost? _host;
        private IntegrationMessage? _request;
        private Func<IAdvancedIntegrationContext, CancellationToken, Task>? _messageHandler;
        private IDependencyContainer? _dependencyContainer;
        private IMessageHandlerMiddlewareComposite? _messageHandlerMiddleware;
        private IMessageHandlerMiddleware? _tracingMiddleware;
        private IMessageHandlerMiddleware? _errorHandlingMiddleware;
        private IMessageHandlerMiddleware? _authorizationMiddleware;
        private IMessageHandlerMiddleware? _unitOfWorkMiddleware;
        private IMessageHandlerMiddleware? _handledByEndpointMiddleware;
        private IMessageHandlerMiddleware? _requestReplyMiddleware;

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            var solutionFileDirectory = SolutionExtensions.SolutionFile().Directory
                                        ?? throw new InvalidOperationException("Solution directory wasn't found");

            var settingsDirectory = solutionFileDirectory
                .StepInto("tests")
                .StepInto("Benchmarks")
                .StepInto("GenericHost.Benchmark")
                .StepInto("Settings")
                .StepInto(nameof(MessageHandlerMiddlewareBenchmarkSource));

            var transportIdentity = IntegrationTransport.InMemory.Identity.TransportIdentity();

            var endpointIdentity = new EndpointIdentity(
                nameof(MessageHandlerMiddlewareBenchmarkSource),
                Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Unable to get entry assembly"));

            _host = new TestFixture()
                .CreateHostBuilder()
                .UseInMemoryIntegrationTransport(transportIdentity)
                .UseEndpoint(endpointIdentity,
                    builder => builder
                        .WithPostgreSqlDataAccess(options => options
                            .ExecuteMigrations())
                        .WithSqlEventSourcing()
                        .WithJwtAuthentication(builder.Context.Configuration)
                        .WithAuthorization()
                        .WithOpenTelemetry()
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)))
                        .BuildOptions())
                .UseOpenTelemetry()
                .BuildHost(settingsDirectory);

            _host.StartAsync(_cts.Token).Wait(_cts.Token);

            _dependencyContainer = _host.GetEndpointDependencyContainer(endpointIdentity);

            var username = "qwerty";

            var authorizationToken = _dependencyContainer
                .Resolve<ITokenProvider>()
                .GenerateToken(
                    username,
                    new[]
                    {
                        nameof(MessageHandlerMiddlewareBenchmarkSource)
                    },
                    TimeSpan.FromSeconds(300));

            _request = _dependencyContainer
                .Resolve<IIntegrationMessageFactory>()
                .CreateGeneralMessage(
                    new Request(),
                    typeof(Request),
                    new[] { new Authorization(authorizationToken) },
                    null);

            _messageHandler = static (context, token) => context.Reply((Request)context.Message.Payload, new Reply(), token);

            _messageHandlerMiddleware = _dependencyContainer.Resolve<IMessageHandlerMiddlewareComposite>();

            var middlewares = _dependencyContainer.ResolveCollection<IMessageHandlerMiddleware>().ToList();

            _tracingMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(TracingMiddleware));
            _errorHandlingMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(ErrorHandlingMiddleware));
            _authorizationMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(AuthorizationMiddleware));
            _unitOfWorkMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(UnitOfWorkMiddleware));
            _handledByEndpointMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(HandledByEndpointMiddleware));
            _requestReplyMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(RequestReplyMiddleware));
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
            _ = _dependencyContainer;
        }

        /// <summary>
        /// IterationCleanup
        /// </summary>
        [IterationCleanup]
        public void IterationCleanup()
        {
            var headers = (Dictionary<Type, IIntegrationMessageHeader>)_request.Headers;

            _ = headers.Remove(typeof(ActualDeliveryDate));
            _ = headers.Remove(typeof(DeliveryTag));
            _ = headers.Remove(typeof(HandledBy));
            _ = headers.Remove(typeof(RejectReason));
        }

        /// <summary> RunCompositeMiddleware </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(RunCompositeMiddleware), Baseline = true)]
        public async Task RunCompositeMiddleware()
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_request!);

                await _messageHandlerMiddleware
                    .Handle(exclusiveContext, _messageHandler!, _cts.Token)
                    .ConfigureAwait(false);
            }
        }

        /// <summary> RunTracingMiddleware </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(RunTracingMiddleware))]
        public async Task RunTracingMiddleware()
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_request!);

                await _tracingMiddleware
                    .Handle(exclusiveContext, _messageHandler!, _cts.Token)
                    .ConfigureAwait(false);
            }
        }

        /// <summary> RunErrorHandlingMiddleware </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(RunErrorHandlingMiddleware))]
        public async Task RunErrorHandlingMiddleware()
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_request!);

                await _errorHandlingMiddleware
                    .Handle(exclusiveContext, _messageHandler!, _cts.Token)
                    .ConfigureAwait(false);
            }
        }

        /// <summary> RunAuthorizationMiddleware </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(RunAuthorizationMiddleware))]
        public async Task RunAuthorizationMiddleware()
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_request!);

                await _authorizationMiddleware
                    .Handle(exclusiveContext, _messageHandler!, _cts.Token)
                    .ConfigureAwait(false);
            }
        }

        /// <summary> RunUnitOfWorkMiddleware </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(RunUnitOfWorkMiddleware))]
        public async Task RunUnitOfWorkMiddleware()
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_request!);

                await _unitOfWorkMiddleware
                    .Handle(exclusiveContext, _messageHandler!, _cts.Token)
                    .ConfigureAwait(false);
            }
        }

        /// <summary> RunHandledByEndpointMiddleware </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(RunHandledByEndpointMiddleware))]
        public async Task RunHandledByEndpointMiddleware()
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_request!);

                await _handledByEndpointMiddleware
                    .Handle(exclusiveContext, _messageHandler!, _cts.Token)
                    .ConfigureAwait(false);
            }
        }

        /// <summary> RunRequestReplyMiddleware </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(RunRequestReplyMiddleware))]
        public async Task RunRequestReplyMiddleware()
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_request!);

                await _requestReplyMiddleware
                    .Handle(exclusiveContext, _messageHandler!, _cts.Token)
                    .ConfigureAwait(false);
            }
        }
    }
}