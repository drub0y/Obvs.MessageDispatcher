using System;

namespace Obvs.MessageDispatcher
{
    public static class ObservableExtensions
    {
        public static IDisposable DispatchMessages<TMessage>(this IObservable<TMessage> messages, Func<IMessageHandlerSelector> messageHandlerSelectorFactory)
        {
            return ObservableExtensions.CreateMessageDispatcher<TMessage>(messages, messageHandlerSelectorFactory).Subscribe();
        }

        public static IDisposable DispatchMessages<TMessage>(this IObservable<TMessage> messages, Func<IMessageHandlerSelector> messageHandlerSelectorFactory, Action<MessageDispatchResult<TMessage>> onNextMessageDispatchResult)
        {
            return ObservableExtensions.CreateMessageDispatcher<TMessage>(messages, messageHandlerSelectorFactory).Subscribe(onNextMessageDispatchResult);
        }

        public static IDisposable DispatchMessages<TMessage>(this IObservable<TMessage> messages, Func<IMessageHandlerSelector> messageHandlerSelectorFactory, Action<MessageDispatchResult<TMessage>> onNextMessageDispatchResult, Action<Exception> onError, Action onCompleted)
        {
            return ObservableExtensions.CreateMessageDispatcher<TMessage>(messages, messageHandlerSelectorFactory).Subscribe(onNextMessageDispatchResult, onError, onCompleted);
        }

        public static IDisposable DispatchMessages<TMessage>(this IObservable<TMessage> messages, Func<IMessageHandlerSelector> messageHandlerSelectorFactory, Action<MessageDispatchResult<TMessage>> onNextMessageDispatchResult, IObserver<MessageDispatchResult<TMessage>> observer)
        {
            return ObservableExtensions.CreateMessageDispatcher<TMessage>(messages, messageHandlerSelectorFactory).Subscribe(observer);
        }

        private static IObservable<MessageDispatchResult<TMessage>> CreateMessageDispatcher<TMessage>(IObservable<TMessage> messages, Func<IMessageHandlerSelector> messageHandlerSelectorFactory) => new MessageDispatcher<TMessage>(messages, messageHandlerSelectorFactory);
    }
}
