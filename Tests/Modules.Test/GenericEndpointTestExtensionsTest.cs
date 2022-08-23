namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.TestExtensions;
    using GenericEndpoint.TestExtensions.Internals;
    using MessageHandlers;
    using Messages;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// GenericEndpoint.TestExtensions test
    /// </summary>
    public class GenericEndpointTestExtensionsTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public GenericEndpointTestExtensionsTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        internal void MessageHandlerTestExtensionsTest()
        {
            {
                var context = new TestIntegrationContext();

                new CommandEmptyMessageHandler(TestIdentity.Endpoint10, context)
                    .OnMessage(new Command(42))
                    .Publishes<Endpoint1HandlerInvoked>(invoked =>
                        invoked.EndpointIdentity.Equals(TestIdentity.Endpoint10)
                        && invoked.HandlerType == typeof(CommandEmptyMessageHandler))
                    .DoesNotThrow()
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new CommandThrowingMessageHandler()
                    .OnMessage(new Command(42))
                    .ProducesNothing()
                    .Throws<InvalidOperationException>(ex => ex.Message == "42")
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new EventEmptyMessageHandler(TestIdentity.Endpoint20, context)
                    .OnMessage(new Event(42))
                    .Publishes<Endpoint2HandlerInvoked>(invoked =>
                        invoked.EndpointIdentity.Equals(TestIdentity.Endpoint20)
                        && invoked.HandlerType == typeof(EventEmptyMessageHandler))
                    .DoesNotThrow()
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new QueryOddReplyMessageHandler(context)
                    .OnMessage(new Query(42))
                    .ProducesNothing()
                    .DoesNotThrow()
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new QueryOddReplyMessageHandler(context)
                    .OnMessage(new Query(43))
                    .DoesNotSend<IIntegrationCommand>()
                    .DoesNotPublish<IIntegrationEvent>()
                    .DoesNotRequest<Query, Reply>()
                    .Replies<Reply>(reply => reply.Id == 43)
                    .DoesNotThrow()
                    .Invoke(context);
            }
        }
    }
}