using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Obvs.MessageDispatcher
{
    internal sealed class DefaultMessageHandlerSelector : IDefaultMessageHandlerSelector
    {
        internal Dictionary<Type, Func<IMessageHandler>> _messageHandlerTypesByHandledMessageType = new Dictionary<Type, Func<IMessageHandler>>();

        public DefaultMessageHandlerSelector()
        {
        }

        public void RegisterMessageHandler(Type messageHandlerType)
        {
            ForEachSupportedMessageType(messageHandlerType, supportedMessageType => _messageHandlerTypesByHandledMessageType[supportedMessageType] = Expression.Lambda<Func<IMessageHandler>>(Expression.New(messageHandlerType)).Compile());
        }

        public void RegisterMessageHandler(IMessageHandler messageHandler)
        {
            if(messageHandler == null) throw new ArgumentNullException(nameof(messageHandler));

            ForEachSupportedMessageType(messageHandler.GetType(), supportedMessageType => _messageHandlerTypesByHandledMessageType[supportedMessageType] = () => messageHandler);
        }

        public void RegisterMessageHandler<TMessageHandler>(Func<TMessageHandler> messageHandlerFactory) 
            where TMessageHandler : class
        {
            if(messageHandlerFactory == null) throw new ArgumentNullException(nameof(messageHandlerFactory));

            ForEachSupportedMessageType(typeof(TMessageHandler), supportedMessageType => _messageHandlerTypesByHandledMessageType[supportedMessageType] = (Func<IMessageHandler>)messageHandlerFactory);
        }

        public IMessageHandler<TMessage> SelectMessageHandler<TMessage>(TMessage message)
        {
            Func<IMessageHandler> messageHandlerFactoryForMessageType;

            _messageHandlerTypesByHandledMessageType.TryGetValue(typeof(TMessage), out messageHandlerFactoryForMessageType);

            return (IMessageHandler<TMessage>)messageHandlerFactoryForMessageType();
        }

        private void ForEachSupportedMessageType(Type messageHandlerType, Action<Type> action)
        {
            if(messageHandlerType == null) throw new ArgumentNullException(nameof(messageHandlerType));
            if(messageHandlerType.GetConstructor(Type.EmptyTypes) == null) throw new ArgumentException($"The specified message handler type, ${messageHandlerType.Name}, does not contain an empty constructor.", nameof(messageHandlerType));

            foreach(Type messageType in messageHandlerType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)).SelectMany(i => i.GetGenericArguments()))
            {
                action(messageType);
            }
        }
    }
}

