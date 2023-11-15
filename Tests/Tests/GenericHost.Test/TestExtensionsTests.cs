namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.TestExtensions;
    using GenericEndpoint.TestExtensions.Internals;
    using MessageHandlers;
    using Messages;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;
    using EventHandler = MessageHandlers.EventHandler;

    /// <summary>
    /// GenericEndpoint.TestExtensions test
    /// </summary>
    public class TestExtensionsTests : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public TestExtensionsTests(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        internal void Assert_message_handler_test_extensions()
        {
            {
                var context = new TestIntegrationContext();

                new CommandHandler(TestIdentity.Endpoint10, context)
                    .OnMessage(new Command(42))
                    .Publishes<HandlerInvoked>(invoked =>
                        invoked.EndpointIdentity == TestIdentity.Endpoint10
                        && invoked.HandlerType == typeof(CommandHandler))
                    .DoesNotThrow()
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new ThrowingCommandHandler()
                    .OnMessage(new Command(42))
                    .ProducesNothing()
                    .Throws<InvalidOperationException>(ex => ex.Message == "42")
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new EventHandler(TestIdentity.Endpoint20, context)
                    .OnMessage(new Event(42))
                    .Publishes<HandlerInvoked>(invoked =>
                        invoked.EndpointIdentity == TestIdentity.Endpoint20
                        && invoked.HandlerType == typeof(EventHandler))
                    .DoesNotThrow()
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new OddReplyRequestHandler(context)
                    .OnMessage(new Request(42))
                    .ProducesNothing()
                    .DoesNotThrow()
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new OddReplyRequestHandler(context)
                    .OnMessage(new Request(43))
                    .DoesNotSend<IIntegrationCommand>()
                    .DoesNotDelay<IIntegrationCommand>()
                    .DoesNotPublish<IIntegrationEvent>()
                    .DoesNotRequest<Request, Reply>()
                    .Replies<Reply>(reply => reply.Id == 43)
                    .DoesNotThrow()
                    .Invoke(context);
            }

            {
                var context = new TestIntegrationContext();

                new SendDelayedCommandEventHandler(context)
                    .OnMessage(new Event(42))
                    .DoesNotSend<IIntegrationCommand>()
                    .Delays<Command>((command, dateTime) =>
                        command.Id == 42 &&
                        (int)Math.Round((dateTime.ToUniversalTime() - DateTime.UtcNow).TotalDays, MidpointRounding.AwayFromZero) == 42)
                    .DoesNotPublish<IIntegrationEvent>()
                    .DoesNotRequest<Request, Reply>()
                    .DoesNotReply<Reply>()
                    .DoesNotThrow()
                    .Invoke(context);
            }
        }
    }
}