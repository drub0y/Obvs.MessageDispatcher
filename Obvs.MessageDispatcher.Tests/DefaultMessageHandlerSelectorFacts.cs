using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Obvs.MessageDispatcher.Tests
{
    public class DefaultMessageHandlerSelectorFacts
    {
        public class ConstructorFacts
        {
            [Fact]
            public void CanCreateANewInstanceWithNoArguments()
            {
                new DefaultMessageHandlerSelector();
            }
        }

        public class RegisterMessageHandlerTypeFacts
        {
            [Fact]
            public void RegisterSingleMessageHandlerByType()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                defaultMessageHandlerSelector.RegisterMessageHandler(typeof(TestMessageHandler));

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().NotBeNull();
            }

            [Fact]
            public void RegisterMultipleMessageHandlersByType()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                defaultMessageHandlerSelector.RegisterMessageHandler(typeof(TestMessageHandler));
                defaultMessageHandlerSelector.RegisterMessageHandler(typeof(TestMessage2Handler));

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().NotBeNull();

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage2)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage2)].Should().NotBeNull();
            }

            [Fact]
            public void RegisterMultipleMessageHandlersByType_ThatHandleSameMessageType_LastRegistrationWins()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                defaultMessageHandlerSelector.RegisterMessageHandler(typeof(TestMessageHandler));
                defaultMessageHandlerSelector.RegisterMessageHandler(typeof(AnotherTestMessageHandler));

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)]().Should().BeOfType<AnotherTestMessageHandler>();
            }
        }

        public class RegisterMessageHandlerInstanceFacts
        {
            [Fact]
            public void RegisterSingleMessageHandler()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                var testMessageHandler = new TestMessageHandler();
                defaultMessageHandlerSelector.RegisterMessageHandler(testMessageHandler);

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().NotBeNull();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)]().Should().BeSameAs(testMessageHandler);
            }

            [Fact]
            public void RegisterMultipleMessageHandlersByType()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                var testMessageHandler = new TestMessageHandler();
                defaultMessageHandlerSelector.RegisterMessageHandler(testMessageHandler);

                var testMessage2Handler = new TestMessage2Handler();
                defaultMessageHandlerSelector.RegisterMessageHandler(testMessage2Handler);

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().NotBeNull();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)]().Should().BeSameAs(testMessageHandler);

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage2)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage2)].Should().NotBeNull();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage2)]().Should().BeSameAs(testMessage2Handler);
            }

            [Fact]
            public void RegisterMultipleMessageHandlersByType_ThatHandleSameMessageType_LastRegistrationWins()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                defaultMessageHandlerSelector.RegisterMessageHandler(new TestMessageHandler());

                var expectedMessageHandler = new AnotherTestMessageHandler();
                defaultMessageHandlerSelector.RegisterMessageHandler(expectedMessageHandler);

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)]().Should().BeSameAs(expectedMessageHandler);
            }
        }

        public class RegisterMessageHandlerFactoryMethodFacts
        {
            [Fact]
            public void RegisterSingleMessageHandlerFactory()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                Func<TestMessageHandler> testMessageHandlerFactory = () => new TestMessageHandler();
                defaultMessageHandlerSelector.RegisterMessageHandler(testMessageHandlerFactory);

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().BeSameAs(testMessageHandlerFactory);
            }

            [Fact]
            public void RegisterMultipleMessageHandlerFactories()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                Func<TestMessageHandler> testMessageHandlerFactory = () => new TestMessageHandler();
                defaultMessageHandlerSelector.RegisterMessageHandler(testMessageHandlerFactory);

                Func<TestMessage2Handler> testMessage2HandlerFactory = () => new TestMessage2Handler();
                defaultMessageHandlerSelector.RegisterMessageHandler(testMessage2HandlerFactory);

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().BeSameAs(testMessageHandlerFactory);

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage2)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage2)].Should().BeSameAs(testMessage2HandlerFactory);
            }

            [Fact]
            public void RegisterMultipleMessageHandlerFactories_ThatHandleSameMessageType_LastRegistrationWins()
            {
                var defaultMessageHandlerSelector = new DefaultMessageHandlerSelector();

                defaultMessageHandlerSelector.RegisterMessageHandler(() => new TestMessageHandler());

                Func<AnotherTestMessageHandler> expectedMessageHandlerFactory = () => new AnotherTestMessageHandler();
                defaultMessageHandlerSelector.RegisterMessageHandler(expectedMessageHandlerFactory);

                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType.ContainsKey(typeof(TestMessage)).Should().BeTrue();
                defaultMessageHandlerSelector._messageHandlerTypesByHandledMessageType[typeof(TestMessage)].Should().BeSameAs(expectedMessageHandlerFactory);
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
