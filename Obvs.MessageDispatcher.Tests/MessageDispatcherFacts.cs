﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Obvs.Types;
using Xunit;

namespace Obvs.MessageDispatcher.Tests
{
    public class MessageDispatcherFacts
    {
        public class RunFacts
        {
            [Fact]
            public void SubscribesToObservableMessageSource()
            {
                var testMessagesMock = new Mock<IObservable<TestMessage>>();
                testMessagesMock.Setup(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()))
                    .Returns(Disposable.Empty);

                var dispatcher = new MessageDispatcher<TestMessage>(testMessagesMock.Object, () => Mock.Of<IMessageHandlerSelector>());

                var subscription = dispatcher.Subscribe(Mock.Of<IObserver<MessageDispatchResult<TestMessage>>>());

                subscription.Should().NotBeNull();

                testMessagesMock.Verify(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()), Times.Once());
            }

            [Fact]
            public void DisposesSubscriptionOnObservableMessageSource()
            {
                var sourceSubscription = new Mock<IDisposable>();

                var testMessagesMock = new Mock<IObservable<TestMessage>>();
                testMessagesMock.Setup(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()))
                    .Returns(sourceSubscription.Object);

                var dispatcher = new MessageDispatcher<TestMessage>(testMessagesMock.Object, () => Mock.Of<IMessageHandlerSelector>());


                var subscription = dispatcher.Subscribe(Mock.Of<IObserver<MessageDispatchResult<TestMessage>>>());

                subscription.Dispose();

                sourceSubscription.Verify(it => it.Dispose(), Times.Once());
            }

            [Fact]
            public void InvokesMessageHandlerSelector()
            {
                var mockMessageHandlerSelector = new Mock<IMessageHandlerSelector>();

                var messageSubject = new Subject<TestMessage>();

                var dispatcher = new MessageDispatcher<TestMessage>(messageSubject, () => mockMessageHandlerSelector.Object);

                var testMessage = new TestMessage();

                using(dispatcher.Subscribe())
                {
                    messageSubject.OnNext(testMessage);
                }

                mockMessageHandlerSelector.Verify(mhp => mhp.SelectMessageHandler<TestMessage>(testMessage), Times.Once());
            }

            [Fact]
            public void InvokesNewMessageHandlerSelectorFromFactoryEachMessage()
            {
                var messageHandlerSelectorFactoryMock = new Mock<Func<IMessageHandlerSelector>>();

                var firstMockMessageHandlerSelector = new Mock<IMessageHandlerSelector>();
                var secondMockMessageHandlerSelector = new Mock<IMessageHandlerSelector>();
                var currentMockMessageHandlerSelector = firstMockMessageHandlerSelector;

                messageHandlerSelectorFactoryMock.Setup(f => f())
                    .Returns(() =>
                    {
                        var result = currentMockMessageHandlerSelector;

                        if(Object.ReferenceEquals(currentMockMessageHandlerSelector, firstMockMessageHandlerSelector))
                        {
                            currentMockMessageHandlerSelector = secondMockMessageHandlerSelector;
                        }

                        return result.Object;
                    });

                var messageSubject = new Subject<TestMessage>();

                var dispatcher = new MessageDispatcher<TestMessage>(messageSubject, messageHandlerSelectorFactoryMock.Object);


                var testMessage = new TestMessage();
                var testMessage2 = new TestMessage();

                using(dispatcher.Subscribe())
                {
                    messageSubject.OnNext(testMessage);
                    messageSubject.OnNext(testMessage2);
                }

                messageHandlerSelectorFactoryMock.Verify(mhsfs => mhsfs(), Times.Exactly(2));

                firstMockMessageHandlerSelector.Verify(mhp => mhp.SelectMessageHandler<TestMessage>(testMessage), Times.Once());
                secondMockMessageHandlerSelector.Verify(mhp => mhp.SelectMessageHandler<TestMessage>(testMessage2), Times.Once());
            }

            [Fact]
            public void DisposesOfDisposableMessageHandlerSelectorEachMessage()
            {
                var mockMessageHandlerSelector = new Mock<IMessageHandlerSelector>();

                mockMessageHandlerSelector.As<IDisposable>();

                var messageSubject = new Subject<TestMessage>();

                var dispatcher = new MessageDispatcher<TestMessage>(messageSubject, () => mockMessageHandlerSelector.Object);

                var testMessage = new TestMessage();

                using(dispatcher.Subscribe())
                {
                    messageSubject.OnNext(testMessage);
                    messageSubject.OnNext(testMessage);
                }

                mockMessageHandlerSelector.As<IDisposable>().Verify(d => d.Dispose(), Times.Exactly(2));
            }


            [Fact]
            public void DoesntDispatchMessageWhichHasNoHandler()
            {
                var messageSubject = new Subject<TestMessage>();

                var dispatcher = new MessageDispatcher<TestMessage>(messageSubject, () => Mock.Of<IMessageHandlerSelector>());

                TestMessage testMessage = new TestMessage();
                MessageDispatchResult<TestMessage> expectedMessageDispatchResult = null;

                using(dispatcher.Subscribe(mdr =>
                   {
                       expectedMessageDispatchResult = mdr;
                   }))
                {
                    messageSubject.OnNext(testMessage);
                }

                expectedMessageDispatchResult.Should().NotBeNull();

                expectedMessageDispatchResult.Message.Should().BeSameAs(testMessage);
                expectedMessageDispatchResult.Handled.Should().Be(false);
            }

            [Fact]
            public void InvokesHandleAsyncOnMessageHandler()
            {
                Mock<IMessageHandler<TestMessage>> mockMessageHandler = new Mock<IMessageHandler<TestMessage>>();

                Mock<IMessageHandlerSelector> mockMessageHandlerSelector = new Mock<IMessageHandlerSelector>();
                mockMessageHandlerSelector.Setup(mhp => mhp.SelectMessageHandler<TestMessage>(It.IsAny<TestMessage>()))
                    .Returns(mockMessageHandler.Object);

                var messageSubject = new Subject<TestMessage>();

                var dispatcher = new MessageDispatcher<TestMessage>(messageSubject, () => mockMessageHandlerSelector.Object);

                var testMessage = new TestMessage();

                using(dispatcher.Subscribe())
                {
                    messageSubject.OnNext(testMessage);
                }

                mockMessageHandler.Verify(mh => mh.HandleAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void DoesNotHandleMessagesBeforeOrAfterSubscription()
            {
                var mockMessageHandler = new Mock<IMessageHandler<TestMessage>>();

                var mockMessageHandlerSelector = new Mock<IMessageHandlerSelector>();
                mockMessageHandlerSelector.Setup(mhp => mhp.SelectMessageHandler<TestMessage>(It.IsAny<TestMessage>()))
                    .Returns(mockMessageHandler.Object);

                var messageSubject = new Subject<TestMessage>();

                var dispatcher = new MessageDispatcher<TestMessage>(messageSubject, () => mockMessageHandlerSelector.Object);

                var testMessage = new TestMessage();

                messageSubject.OnNext(testMessage);

                using(dispatcher.Subscribe())
                {
                    messageSubject.OnNext(testMessage);
                }

                messageSubject.OnNext(testMessage);

                mockMessageHandler.Verify(mh => mh.HandleAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public async Task ProcessesMessagesInCorrectOrder()
            {
                var mockMessageHandler = new Mock<IMessageHandler<TestMessage>>();

                var mockMessageHandlerSelector = new Mock<IMessageHandlerSelector>();
                mockMessageHandlerSelector.Setup(mhp => mhp.SelectMessageHandler<TestMessage>(It.IsAny<TestMessage>()))
                    .Returns(mockMessageHandler.Object);

                TestMessage testMessage1 = new TestMessage();
                TestMessage testMessage2 = new TestMessage();

                var testMessages = (new[] { testMessage1, testMessage2 }).ToObservable();

                var dispatcher = new MessageDispatcher<TestMessage>(testMessages, () => mockMessageHandlerSelector.Object);

                bool areEqual = await dispatcher.Select(mdr => mdr.Message).SequenceEqual(testMessages).FirstAsync();

                areEqual.Should().Be(true);
            }
        }

        public class TestMessage : IMessage
        {

        }
    }
}
