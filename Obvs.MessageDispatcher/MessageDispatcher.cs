using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Obvs.MessageDispatcher
{
    public class MessageDispatcher<TMessage> : IMessageDispatcher<TMessage>
    {
        private static readonly MethodInfo MessageHandlerProviderGetMessageHandlerGenericMethodInfo = typeof(IMessageHandlerProvider).GetMethod(nameof(IMessageHandlerProvider.GetMessageHandler));

        private readonly Func<IMessageHandlerProvider> _handlerProviderFactory;
        private readonly ConcurrentDictionary<Type, Func<IMessageHandler>> _messageHandlerGetTypedHandlerFuncCache = new ConcurrentDictionary<Type, Func<IMessageHandler>>();
        private readonly ConcurrentDictionary<Type, Func<IMessageHandler, TMessage, CancellationToken, Task>> _messageHandlerHandleAsyncFuncCache = new ConcurrentDictionary<Type, Func<IMessageHandler, TMessage, CancellationToken, Task>>();

        public MessageDispatcher(Func<IMessageHandlerProvider> handlerProviderFactory)
        {
            _handlerProviderFactory = handlerProviderFactory;
        }

        public IObservable<MessageDispatchResult<TMessage>> Run(IObservable<TMessage> messages)
        {
            return messages.SelectMany(async (message, cancellationToken) =>
            {
                var messageType = message.GetType();

                var messageHandlerProvider = _handlerProviderFactory();

                try
                {
                    var messageHandler = GetHandlerForMessageType(messageHandlerProvider, messageType);
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
                    var disposableMessageHandlerProvider = messageHandlerProvider as IDisposable;

                    if(disposableMessageHandlerProvider != null)
                    {
                        disposableMessageHandlerProvider.Dispose();
                    }
                }
            })
            .Select(mdr => mdr);
        }

        private IMessageHandler GetHandlerForMessageType(IMessageHandlerProvider messageHandlerProvider, Type messageType)
        {
            var getMessageHandlerFromProviderTypedFunc = _messageHandlerGetTypedHandlerFuncCache.GetOrAdd(
                messageType,
                mt =>
                {
                    var getHandlersOfMessageTypeMethodInfo = MessageHandlerProviderGetMessageHandlerGenericMethodInfo.MakeGenericMethod(mt);
                    var messageHandlerProviderConstantExpression = Expression.Constant(messageHandlerProvider, typeof(IMessageHandlerProvider));

                    return Expression.Lambda<Func<IMessageHandler>>(Expression.Call(messageHandlerProviderConstantExpression, getHandlersOfMessageTypeMethodInfo)).Compile();
                });

            return getMessageHandlerFromProviderTypedFunc();
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
