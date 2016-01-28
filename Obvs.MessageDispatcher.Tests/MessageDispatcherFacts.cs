using System;
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
				var mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();

				var dispatcher = new MessageDispatcher<TestMessage>(() => mockMessageHandlerProvider.Object);

				var testMessagesMock = new Mock<IObservable<TestMessage>>();
				testMessagesMock.Setup(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()))
					.Returns(Disposable.Empty);

				var messageDispatchResults = dispatcher.Run(testMessagesMock.Object);

				messageDispatchResults.Should().NotBeNull();

				messageDispatchResults.Subscribe();

				testMessagesMock.Verify(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()), Times.Once());
			}

			[Fact]
			public void DisposesSubscriptionOnObservableMessageSource()
			{
				var mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();

				var dispatcher = new MessageDispatcher<TestMessage>(() => mockMessageHandlerProvider.Object);

				var runAsyncDisposableMock = new Mock<IDisposable>();

				var testMessagesMock = new Mock<IObservable<TestMessage>>();
				testMessagesMock.Setup(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()))
					.Returns(runAsyncDisposableMock.Object);

				var messageDispatchResults = dispatcher.Run(testMessagesMock.Object);

				messageDispatchResults.Subscribe().Dispose();

				runAsyncDisposableMock.Verify(it => it.Dispose(), Times.Once());
			}

			[Fact]
			public void InvokesMessageHandlerProvider()
			{
				var mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();

				var dispatcher = new MessageDispatcher<TestMessage>(() => mockMessageHandlerProvider.Object);

				var messageSubject = new Subject<TestMessage>();

				using (dispatcher.Run(messageSubject).Subscribe())
				{
					messageSubject.OnNext(new TestMessage());
				}
				
				mockMessageHandlerProvider.Verify(mhp => mhp.GetMessageHandler<TestMessage>(), Times.Once());
			}

			[Fact]
			public void DoesntDispatchMessageWhichHasNoHandler()
			{
				var mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();

                var dispatcher = new MessageDispatcher<TestMessage>(() => mockMessageHandlerProvider.Object);

				var messageSubject = new Subject<TestMessage>();

				var messageDispatchResults = dispatcher.Run(messageSubject);

				TestMessage testMessage = new TestMessage();

				using (messageDispatchResults.Subscribe(mdr =>
				{
					mdr.Message.Should().BeSameAs(testMessage);
					mdr.Handled.Should().Be(false);
				}))
				{
					messageSubject.OnNext(testMessage);
				}
			}

			[Fact]
			public void InvokesHandleAsyncOnMessageHandler()
			{
				Mock<IMessageHandler<TestMessage>> mockMessageHandler = new Mock<IMessageHandler<TestMessage>>();

				Mock<IMessageHandlerProvider> mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();
				mockMessageHandlerProvider.Setup(mhp => mhp.GetMessageHandler<TestMessage>())
					.Returns(mockMessageHandler.Object);

				var dispatcher = new MessageDispatcher<TestMessage>(() => mockMessageHandlerProvider.Object);

				var messageSubject = new Subject<TestMessage>();

				var testMessage = new TestMessage();

				var messageDispatcherResults = dispatcher.Run(messageSubject);

				using(messageDispatcherResults.Subscribe(mdr =>
				{
					mdr.Message.Should().BeSameAs(testMessage);
					mdr.Handled.Should().Be(true);
				}))
				{
					messageSubject.OnNext(testMessage);
				}

				mockMessageHandler.Verify(mh => mh.HandleAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once());
			}

			[Fact]
			public void DoesNotHandleMessagesBeforeOrAfterSubscription()
			{
				var mockMessageHandler = new Mock<IMessageHandler<TestMessage>>();

				var mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();
				mockMessageHandlerProvider.Setup(mhp => mhp.GetMessageHandler<TestMessage>())
					.Returns(mockMessageHandler.Object);

				var dispatcher = new MessageDispatcher<TestMessage>(() => mockMessageHandlerProvider.Object);

				var messageSubject = new Subject<TestMessage>();

				var testMessage = new TestMessage();

				var messageDispatcherResults = dispatcher.Run(messageSubject);

				messageSubject.OnNext(testMessage);

				using (messageDispatcherResults.Subscribe())
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

				var mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();
				mockMessageHandlerProvider.Setup(mhp => mhp.GetMessageHandler<TestMessage>())
					.Returns(mockMessageHandler.Object);

				var dispatcher = new MessageDispatcher<TestMessage>(() => mockMessageHandlerProvider.Object);

				TestMessage testMessage1 = new TestMessage();
				TestMessage testMessage2 = new TestMessage();

				var testMessages = (new[] { testMessage1, testMessage2 }).ToObservable();

				var messageDispatcherResults = dispatcher.Run(testMessages);

				bool areEqual = await messageDispatcherResults.Select(mdr => mdr.Message).SequenceEqual(testMessages).FirstAsync();

				areEqual.Should().Be(true);
			}
		}

		public class TestMessage : IMessage
		{

		}
	}
}
