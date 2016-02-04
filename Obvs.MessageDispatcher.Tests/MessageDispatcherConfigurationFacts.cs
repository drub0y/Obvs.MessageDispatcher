using System;
using FluentAssertions;
using Moq;
using Obvs.MessageDispatcher.Configuration;
using Obvs.Types;
using Xunit;

namespace Obvs.MessageDispatcher.Tests
{
    public class MessageDispatcherConfigurationFacts
    {
        public class ConstructorFacts
        {
            [Fact]
            public void ThrowsOnNullMessagesObservable()
            {
                Action action = () => new MessageDispatcherConfiguration<IMessage>(null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("messages");
            }
        }

        public class WithMessageHandlerSelectorFactoryFacts
        {
            [Fact]
            public void ThrowsOnNullFactory()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.DispatcherFor(sb => sb.Commands);

                Action action = () => messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("messageHandlerSelectorFactory");
            }

            [Fact]
            public void ReturnsMessageDispatcherConfiguration()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.DispatcherFor(sb => sb.Commands);

                var updatedConfiguration = messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>());

                updatedConfiguration.Should().NotBeNull();
            }

            [Fact]
            public void MessageDispatcherConfigurationMessageHandlerShouldBeTheConfiguredFactory()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.DispatcherFor(sb => sb.Commands);

                Func<IMessageHandlerSelector> messageHandlerSelectorFactory = () => Mock.Of<IMessageHandlerSelector>();

                var updatedConfiguration = messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(messageHandlerSelectorFactory);

                ((MessageDispatcherConfiguration<ICommand>)updatedConfiguration).MessageHandlerSelectorFactory.Should().Be(messageHandlerSelectorFactory);
            }
        }

        public class RunDispatcherFacts
        {
            [Fact]
            public void ReturnsNonNullObservable()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.DispatcherFor(sb => sb.Commands);

                var messageDispatchResults = messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>())
                    .RunDispatcher();

                messageDispatchResults.Should().NotBeNull();
            }
        }
    }

    public class DispatcherForFacts
    {
        [Fact]
        public void ThrowsOnNullServiceBus()
        {
            IServiceBus serviceBus = null;

            Action action = () => serviceBus.DispatcherFor(sb => sb.Commands);

            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serviceBus");
        }

        [Fact]
        public void ReturnsMessageDispatcherConfiguration()
        {
            var serviceBusMock = new Mock<IServiceBus>();
            serviceBusMock.SetupGet(sb => sb.Commands)
                .Returns(Mock.Of<IObservable<ICommand>>());

            var messageDispatcherConfiguration = serviceBusMock.Object.DispatcherFor(sb => sb.Commands);

            messageDispatcherConfiguration.Should().NotBeNull();
        }

        [Fact]
        public void MessageDispatcherConfigurationMessagesShouldBeTheSelectedMessages()
        {
            var commandsObservableMock = Mock.Of<IObservable<ICommand>>();

            var serviceBusMock = new Mock<IServiceBus>();
            serviceBusMock.SetupGet(sb => sb.Commands)
                .Returns(commandsObservableMock);

            var messageDispatcherConfiguration = serviceBusMock.Object.DispatcherFor(sb => sb.Commands);

            ((MessageDispatcherConfiguration<ICommand>)messageDispatcherConfiguration).Messages.Should().Be(commandsObservableMock);
        }
    }
}
