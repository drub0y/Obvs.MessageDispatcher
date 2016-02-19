using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Obvs.MessageDispatcher.Tests
{
    public class SimpleMessageHandlerSelectorFacts
    {
        public class ConstructorFacts
        {
            [Fact]
            public void CanCreateANewInstanceWithNoArguments()
            {
                new SimpleMessageHandlerSelector();
            }
        }

        public class RegisterMessageHandlerTypeFacts
        {
            [Fact]
            public void RegisterSingleMessageHandlerByType()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                simpleMessageHandlerSelector.RegisterMessageHandler(typeof(TestMessageHandler));

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().NotBeNull();
            }

            [Fact]
            public void RegisterMultipleMessageHandlersByType()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                simpleMessageHandlerSelector.RegisterMessageHandler(typeof(TestMessageHandler));
                simpleMessageHandlerSelector.RegisterMessageHandler(typeof(TestMessage2Handler));

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().NotBeNull();

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage2)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage2)].Should().NotBeNull();
            }

            [Fact]
            public void RegisterMultipleMessageHandlersByType_ThatHandleSameMessageType_LastRegistrationWins()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                simpleMessageHandlerSelector.RegisterMessageHandler(typeof(TestMessageHandler));
                simpleMessageHandlerSelector.RegisterMessageHandler(typeof(AnotherTestMessageHandler));

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)]().Should().BeOfType<AnotherTestMessageHandler>();
            }
        }

        public class RegisterMessageHandlerInstanceFacts
        {
            [Fact]
            public void RegisterSingleMessageHandler()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                var testMessageHandler = new TestMessageHandler();
                simpleMessageHandlerSelector.RegisterMessageHandler(testMessageHandler);

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().NotBeNull();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)]().Should().BeSameAs(testMessageHandler);
            }

            [Fact]
            public void RegisterMultipleMessageHandlersByType()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                var testMessageHandler = new TestMessageHandler();
                simpleMessageHandlerSelector.RegisterMessageHandler(testMessageHandler);

                var testMessage2Handler = new TestMessage2Handler();
                simpleMessageHandlerSelector.RegisterMessageHandler(testMessage2Handler);

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().NotBeNull();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)]().Should().BeSameAs(testMessageHandler);

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage2)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage2)].Should().NotBeNull();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage2)]().Should().BeSameAs(testMessage2Handler);
            }

            [Fact]
            public void RegisterMultipleMessageHandlersByType_ThatHandleSameMessageType_LastRegistrationWins()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                simpleMessageHandlerSelector.RegisterMessageHandler(new TestMessageHandler());

                var expectedMessageHandler = new AnotherTestMessageHandler();
                simpleMessageHandlerSelector.RegisterMessageHandler(expectedMessageHandler);

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)]().Should().BeSameAs(expectedMessageHandler);
            }
        }

        public class RegisterMessageHandlerFactoryMethodFacts
        {
            [Fact]
            public void RegisterSingleMessageHandlerFactory()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                Func<TestMessageHandler> testMessageHandlerFactory = () => new TestMessageHandler();
                simpleMessageHandlerSelector.RegisterMessageHandler(testMessageHandlerFactory);

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().BeSameAs(testMessageHandlerFactory);
            }

            [Fact]
            public void RegisterMultipleMessageHandlerFactories()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                Func<TestMessageHandler> testMessageHandlerFactory = () => new TestMessageHandler();
                simpleMessageHandlerSelector.RegisterMessageHandler(testMessageHandlerFactory);

                Func<TestMessage2Handler> testMessage2HandlerFactory = () => new TestMessage2Handler();
                simpleMessageHandlerSelector.RegisterMessageHandler(testMessage2HandlerFactory);

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().BeSameAs(testMessageHandlerFactory);

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage2)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage2)].Should().BeSameAs(testMessage2HandlerFactory);
            }

            [Fact]
            public void RegisterMultipleMessageHandlerFactories_ThatHandleSameMessageType_LastRegistrationWins()
            {
                var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

                simpleMessageHandlerSelector.RegisterMessageHandler(() => new TestMessageHandler());

                Func<AnotherTestMessageHandler> expectedMessageHandlerFactory = () => new AnotherTestMessageHandler();
                simpleMessageHandlerSelector.RegisterMessageHandler(expectedMessageHandlerFactory);

                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                simpleMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().BeSameAs(expectedMessageHandlerFactory);
            }
        }


        private sealed class TestMessage
        {
        }

        private sealed class TestMessage2
        {
        }

        private sealed class TestMessageHandler : IMessageHandler<TestMessage>
        {
            public Task HandleAsync(TestMessage message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class AnotherTestMessageHandler : IMessageHandler<TestMessage>
        {
            public Task HandleAsync(TestMessage message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class TestMessage2Handler : IMessageHandler<TestMessage2>
        {
            public Task HandleAsync(TestMessage2 message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
