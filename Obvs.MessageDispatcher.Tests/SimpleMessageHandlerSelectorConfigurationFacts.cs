using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace Obvs.MessageDispatcher.Configuration.Tests
{
    public class SimpleMessageHandlerSelectorConfigurationFacts
    {
        private readonly Mock<ISimpleMessageHandlerSelector> _simpleMessageHandlerSelectorMock;
        private readonly SimpleMessageHandlerSelectorFactoryConfiguration<TestMessage> _simpleMessageHandlerSelectorFactoryConfiguration;

        public SimpleMessageHandlerSelectorConfigurationFacts()
        {
            _simpleMessageHandlerSelectorMock = new Mock<ISimpleMessageHandlerSelector>();
            _simpleMessageHandlerSelectorFactoryConfiguration = new SimpleMessageHandlerSelectorFactoryConfiguration<TestMessage>(_simpleMessageHandlerSelectorMock.Object);
        }

        public class ContructorFacts : SimpleMessageHandlerSelectorConfigurationFacts
        {
            [Fact]
            public void CreateWithValidSimpleMessageHandlerSelectorInstance()
            {
                new SimpleMessageHandlerSelectorFactoryConfiguration<TestMessage>(_simpleMessageHandlerSelectorMock.Object);
            }

            [Fact]
            public void ThrowsOnNullSimpleMessageHandler()
            {
                Action action = () => new SimpleMessageHandlerSelectorFactoryConfiguration<TestMessage>(null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("simpleMessageHandlerSelector");
            }
        }

        public class RegisterMessageHandlerGenericFacts : SimpleMessageHandlerSelectorConfigurationFacts
        {
            [Fact]
            public void RegisterMessageHandler()
            {
                _simpleMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler<TestMessageHandler>();

                _simpleMessageHandlerSelectorMock.Verify(smhs => smhs.RegisterMessageHandler(It.Is<Type>(it => it == typeof(TestMessageHandler))), Times.Once());
            }
        }

        public class RegisterMessageHandlerTypeFacts : SimpleMessageHandlerSelectorConfigurationFacts
        {
            [Fact]
            public void RegisterMessageHandlerType()
            {
                _simpleMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler(typeof(TestMessageHandler));

                _simpleMessageHandlerSelectorMock.Verify(smhs => smhs.RegisterMessageHandler(It.Is<Type>(it => it == typeof(TestMessageHandler))), Times.Once());
            }

            [Fact]
            public void RegisterMessageHandlerType_ThrowsOnNullMessageHandlerType()
            {
                Action action = () => _simpleMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler((Type)null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("messageHandlerType");
            }
        }

        public class RegisterMessageHandlerInstanceFacts : SimpleMessageHandlerSelectorConfigurationFacts
        {
            [Fact]
            public void RegisterMessageHandlerInstance()
            {
                var messageHandler = Mock.Of<IMessageHandler<TestMessage>>();

                _simpleMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler(messageHandler);

                _simpleMessageHandlerSelectorMock.Verify(smhs => smhs.RegisterMessageHandler(It.Is<IMessageHandler>(it => Object.ReferenceEquals(it, messageHandler))), Times.Once());
            }

            [Fact]
            public void RegisterMessageHandlerInstance_ThrowsOnNullMessageHandlerInstance()
            {
                Action action = () => _simpleMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler((IMessageHandler)null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("messageHandler");
            }
        }

        internal class TestMessage
        {

        }

        private class TestMessageHandler : IMessageHandler<TestMessage>
        {
            public Task HandleAsync(TestMessage message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
