using Obvs.Types;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Obvs.MessageDispatcher
{
	public class MessageDispatcher : IMessageDispatcher
	{
		private static readonly MethodInfo GetMessageHandlerMethodInfo = typeof(MessageDispatcher).GetMethod("GetMessageHandler", Type.EmptyTypes);

		private readonly IMessageHandlerProvider _handlerProvider;
		private readonly ConcurrentDictionary<Type, Func<IMessage, CancellationToken, Task>> _messageHandlerHandleAsyncFuncCache = new ConcurrentDictionary<Type, Func<IMessage, CancellationToken, Task>>();
		
		public MessageDispatcher(IMessageHandlerProvider handlerProvider)
		{
			_handlerProvider = handlerProvider;
		}

		public IObservable<MessageDispatchResult> Run(IObservable<IMessage> messages)
		{
			return Run(messages, CancellationToken.None);
		}

		public IObservable<MessageDispatchResult> Run(IObservable<IMessage> messages, CancellationToken cancellationToken)
		{
			return messages.SelectMany(async message =>
				{
					Type messageType = message.GetType();

					Func<IMessage, CancellationToken, Task> handleMessageAsyncFunc = GetMessageHandlerHandleFunc(messageType);
					
					bool messageHandled;

					if (handleMessageAsyncFunc != null)
					{
						await handleMessageAsyncFunc(message, cancellationToken);

						messageHandled = true;
					}
					else
					{
						messageHandled = false;
					}

					return new MessageDispatchResult(message, messageHandled);
				})
				.Select(mdr => mdr);
		}

		public Func<IMessageHandler> GetMessageHandler(Type messageType)
		{
			MethodInfo mi = GetMessageHandlerMethodInfo.MakeGenericMethod(messageType);

			return Expression.Lambda<Func<IMessageHandler>>(Expression.Call(Expression.Constant(this), mi)).Compile();
		}

		private Func<IMessage, CancellationToken, Task> GetMessageHandlerHandleFunc(Type messageType)
		{
			return _messageHandlerHandleAsyncFuncCache.GetOrAdd(
				messageType,
				(mt) =>
				{
					IMessageHandler messageHandler = GetMessageHandler(mt)();
					Func<IMessage, CancellationToken, Task> func;

					if (messageHandler != null)
					{
						MethodInfo handleMethodInfo = typeof(IMessageHandler<>).MakeGenericType(mt).GetMethod("HandleAsync");

						ParameterExpression messageParameterExpression = Expression.Parameter(typeof(IMessage), "message");
						ParameterExpression cancellationTokenParameterExpression = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

						MethodCallExpression handleMethodCallExpression = Expression.Call(Expression.Constant(messageHandler), handleMethodInfo, Expression.Convert(messageParameterExpression, mt), cancellationTokenParameterExpression);

						func = Expression.Lambda<Func<IMessage, CancellationToken, Task>>(handleMethodCallExpression, messageParameterExpression, cancellationTokenParameterExpression).Compile();
					}
					else
					{
						func = null;
					}

					return func;
				});
	    }

		public IMessageHandler<TMessage> GetMessageHandler<TMessage>() where TMessage : IMessage
		{
			return _handlerProvider.Provide<TMessage>();
		}
	}
}
