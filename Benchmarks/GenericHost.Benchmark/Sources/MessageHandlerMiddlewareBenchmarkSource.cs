namespace SpaceEngineers.Core.GenericHost.Benchmark.Sources
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;
    using CompositionRoot;
    using GenericEndpoint.Authorization;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.Contract;
    using GenericEndpoint.DataAccess.Host;
    using GenericEndpoint.EventSourcing;
    using GenericEndpoint.EventSourcing.Host;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
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
        private IntegrationMessage? _message;
        private IDependencyContainer? _dependencyContainer;
        private IMessageHandlerMiddlewareComposite? _messageHandlerMiddleware;
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

            var hostBuilder = new TestFixture().CreateHostBuilder();

            var settingsDirectory = SolutionExtensions
                .SolutionFile()
                .Directory
                .EnsureNotNull("Solution directory wasn't found")
                .StepInto(nameof(Benchmarks))
                .StepInto(AssembliesExtensions.BuildName(nameof(GenericHost), nameof(Benchmark)))
                .StepInto("Settings")
                .StepInto(nameof(MessageHandlerMiddlewareBenchmarkSource));

            _host = hostBuilder
                .UseIntegrationTransport(builder => builder
                    .WithInMemoryIntegrationTransport(hostBuilder)
                    .WithAuthorization()
                    .BuildOptions())
                .UseEndpoint(
                    new EndpointIdentity(nameof(MessageHandlerMiddlewareBenchmarkSource), Guid.NewGuid().ToString()),
                    (_, builder) => builder
                    .WithPostgreSqlDataAccess(options => options.ExecuteMigrations())
                    .WithSqlEventSourcing()
                    .WithAuthorization()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(typeof(RecreatePostgreSqlDatabaseMigration)))
                    .BuildOptions())
                .BuildHost(settingsDirectory);

            _host.StartAsync(_cts.Token).Wait();

            var transportDependencyContainer = _host.GetTransportDependencyContainer();

            var username = "qwerty";

            var authorizationToken = transportDependencyContainer
                .Resolve<ITokenProvider>()
                .GenerateToken(
                    username,
                    new[]
                    {
                        Features.EventSourcing,
                        nameof(MessageHandlerMiddlewareBenchmarkSource)
                    },
                    TimeSpan.FromSeconds(300));

            _message = transportDependencyContainer
                .Resolve<IIntegrationMessageFactory>()
                .CreateGeneralMessage(
                    new Command(),
                    typeof(Command),
                    new[] { new Authorization(authorizationToken) },
                    null);

            _dependencyContainer = _host.GetEndpointDependencyContainer(nameof(MessageHandlerMiddlewareBenchmarkSource));
            _messageHandlerMiddleware = _dependencyContainer.Resolve<IMessageHandlerMiddlewareComposite>();

            var middlewares = _dependencyContainer.ResolveCollection<IMessageHandlerMiddleware>().ToList();

            _errorHandlingMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(ErrorHandlingMiddleware));
            _authorizationMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(AuthorizationMiddleware));
            _unitOfWorkMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(UnitOfWorkMiddleware));
            _handledByEndpointMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(HandledByEndpointMiddleware));
            _requestReplyMiddleware = middlewares.Single(middleware => middleware.GetType() == typeof(RequestReplyMiddleware));
        }

        /// <summary>
        /// IterationCleanup
        /// </summary>
        [IterationCleanup]
        public void IterationCleanup()
        {
            var headers = (Dictionary<Type, IIntegrationMessageHeader>)_message.Headers;

            _ = headers.Remove(typeof(ActualDeliveryDate));
            _ = headers.Remove(typeof(DeliveryTag));
            _ = headers.Remove(typeof(HandledBy));
            _ = headers.Remove(typeof(RejectReason));
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

        /// <summary> RunCompositeMiddleware </summary>
        /// <returns>Ongoing operation</returns>
        [Benchmark(Description = nameof(RunCompositeMiddleware), Baseline = true)]
        public async Task RunCompositeMiddleware()
        {
            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_message!);

                await _messageHandlerMiddleware
                    .Handle(exclusiveContext, static (context, token) => Task.CompletedTask, _cts.Token)
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
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_message!);

                await _errorHandlingMiddleware
                    .Handle(exclusiveContext, static (context, token) => Task.CompletedTask, _cts.Token)
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
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_message!);

                await _authorizationMiddleware
                    .Handle(exclusiveContext, static (context, token) => Task.CompletedTask, _cts.Token)
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
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_message!);

                await _unitOfWorkMiddleware
                    .Handle(exclusiveContext, static (context, token) => Task.CompletedTask, _cts.Token)
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
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_message!);

                await _handledByEndpointMiddleware
                    .Handle(exclusiveContext, static (context, token) => Task.CompletedTask, _cts.Token)
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
                var exclusiveContext = _dependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(_message!);

                await _requestReplyMiddleware
                    .Handle(exclusiveContext, static (context, token) => Task.CompletedTask, _cts.Token)
                    .ConfigureAwait(false);
            }
        }
    }
}