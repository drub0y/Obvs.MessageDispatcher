using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Obvs.MessageDispatcher
{
    internal class MessageDispatcher<TMessage> : IObservable<MessageDispatchResult<TMessage>>
    {
        private static readonly MethodInfo MessageHandlerSelectorSelectMessageHandlerGenericMethodInfo = typeof(IMessageHandlerSelector).GetMethod(nameof(IMessageHandlerSelector.SelectMessageHandler));

        private readonly IObservable<TMessage> _messages;
        private readonly Func<IMessageHandlerSelector> _messageHandlerSelectorFactory;
        private readonly ConcurrentDictionary<Type, Func<IMessageHandlerSelector, TMessage, IMessageHandler>> _messageHandlerGetTypedHandlerFuncCache = new ConcurrentDictionary<Type, Func<IMessageHandlerSelector, TMessage, IMessageHandler>>();
        private readonly ConcurrentDictionary<Type, Func<IMessageHandler, TMessage, CancellationToken, Task>> _messageHandlerHandleAsyncFuncCache = new ConcurrentDictionary<Type, Func<IMessageHandler, TMessage, CancellationToken, Task>>();

        public MessageDispatcher(IObservable<TMessage> messages, Func<IMessageHandlerSelector> messageHandlerSelectorFactory)
        {
            _messages = messages;
            _messageHandlerSelectorFactory = messageHandlerSelectorFactory;
        }


        public IDisposable Subscribe(IObserver<MessageDispatchResult<TMessage>> observer)
        {
            return _messages.SelectMany(async (message, cancellationToken) =>
            {
                var messageHandlerSelector = _messageHandlerSelectorFactory();

                try
                {
                    var messageHandler = SelectHandlerForMessage(messageHandlerSelector, message);
                    var handled = false;

                    if(messageHandler != null)
                    {
                        Func<IMessageHandler, TMessage, CancellationToken, Task> handleMessageAsync = GetMessageHandlerHandleFunc(message.GetType());

                        await handleMessageAsync(messageHandler, message, cancellationToken);

                        handled = true;
                    }

                    return new MessageDispatchResult<TMessage>(message, handled);
                }
                finally
                {
                    var disposableMessageHandlerProvider = messageHandlerSelector as IDisposable;

                    if(disposableMessageHandlerProvider != null)
                    {
                        disposableMessageHandlerProvider.Dispose();
                    }
                }
            })
            .Select(mdr => mdr)
            .Subscribe(observer);
        }

        private IMessageHandler SelectHandlerForMessage(IMessageHandlerSelector messageHandlerSelector, TMessage message)
        {
            var getMessageHandlerFromSelectorTypedFunc = _messageHandlerGetTypedHandlerFuncCache.GetOrAdd(
                message.GetType(),
                mt =>
                {
                    var getHandlerOfMessageTypeMethodInfo = MessageHandlerSelectorSelectMessageHandlerGenericMethodInfo.MakeGenericMethod(mt);
                    var messageHandlerSelectorParameterExpression = Expression.Parameter(typeof(IMessageHandlerSelector));
                    var messageParameterExpression = Expression.Parameter(typeof(TMessage));

                    return Expression.Lambda<Func<IMessageHandlerSelector, TMessage, IMessageHandler>>(Expression.Call(messageHandlerSelectorParameterExpression, getHandlerOfMessageTypeMethodInfo, Expression.Convert(messageParameterExpression, mt)), messageHandlerSelectorParameterExpression, messageParameterExpression).Compile();
                });

            return getMessageHandlerFromSelectorTypedFunc(messageHandlerSelector, message);
        }

        private Func<IMessageHandler, TMessage, CancellationToken, Task> GetMessageHandlerHandleFunc(Type messageType)
        {
            return _messageHandlerHandleAsyncFuncCache.GetOrAdd(
                messageType,
                (mt) =>
                {
                    var messageHandlerGenericType = typeof(IMessageHandler<>).MakeGenericType(mt);
                    var handleMethodInfo = messageHandlerGenericType.GetMethod("HandleAsync");

                    var messageHandlerParameterExpression = Expression.Parameter(typeof(IMessageHandler), "messageHandler");
                    var messageParameterExpression = Expression.Parameter(typeof(TMessage), "message");
                    var cancellationTokenParameterExpression = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                    var handleMethodCallExpression = Expression.Call(Expression.Convert(messageHandlerParameterExpression, messageHandlerGenericType), handleMethodInfo, Expression.Convert(messageParameterExpression, mt), cancellationTokenParameterExpression);

                    var compiledLambda = Expression.Lambda<Func<IMessageHandler, TMessage, CancellationToken, Task>>(handleMethodCallExpression, messageHandlerParameterExpression, messageParameterExpression, cancellationTokenParameterExpression).Compile();

                    return compiledLambda;
                });
        }
    }
}
