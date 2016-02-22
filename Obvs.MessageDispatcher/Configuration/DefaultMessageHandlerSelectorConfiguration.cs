using System;
using System.Linq;
using System.Reflection;

namespace Obvs.MessageDispatcher.Configuration
{
    internal class DefaultMessageHandlerSelectorFactoryConfiguration<TMessage> : IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage>
        where TMessage : class
    {
        private readonly IDefaultMessageHandlerSelector _simpleMessageHandlerSelector;

        public DefaultMessageHandlerSelectorFactoryConfiguration(IDefaultMessageHandlerSelector simpleMessageHandlerSelector)
        {
            if(simpleMessageHandlerSelector == null) throw new ArgumentNullException(nameof(simpleMessageHandlerSelector));

            _simpleMessageHandlerSelector = simpleMessageHandlerSelector;
        }

        public IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler<TMessageHandler>() where TMessageHandler : IMessageHandler
        {
            RegisterMessageHandler(typeof(TMessageHandler));

            return this;
        }

        public IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler(Type messageHandlerType)
        {
            if(messageHandlerType == null) throw new ArgumentNullException(nameof(messageHandlerType));

            _simpleMessageHandlerSelector.RegisterMessageHandler(messageHandlerType);

            return this;
        }

        public IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandlers(params Assembly[] assembliesToScan)
        {
            if(assembliesToScan == null) throw new ArgumentNullException(nameof(assembliesToScan));

            var exportedMessageHandlerTypes = from assembly in assembliesToScan
                                                from type in assembly.GetExportedTypes()
                                                where typeof(IMessageHandler).IsAssignableFrom(type)
                                                                &&
                                                            type.GetConstructor(Type.EmptyTypes) != null
                                                select type;

            foreach(var messageHandlerType in exportedMessageHandlerTypes)
            {
                _simpleMessageHandlerSelector.RegisterMessageHandler(messageHandlerType);
            }

            return this;
        }

        public IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandlers(params Type[] messageHandlerTypes)
        {
            if(messageHandlerTypes == null) throw new ArgumentNullException(nameof(messageHandlerTypes));

            foreach(var messageHandlerType in messageHandlerTypes)
            {
                _simpleMessageHandlerSelector.RegisterMessageHandler(messageHandlerType);
            }

            return this;
        }

        public IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler(IMessageHandler messageHandler)
        {
            if(messageHandler == null) throw new ArgumentNullException(nameof(messageHandler));

            _simpleMessageHandlerSelector.RegisterMessageHandler(messageHandler);

            return this;
        }

        public IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler<TMessageHandler>(Func<TMessageHandler> messageHandler) 
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            _simpleMessageHandlerSelector.RegisterMessageHandler<TMessageHandler>(messageHandler);

            return this;
        }
    }

    public interface IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage>
        where TMessage : class
    {
        IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler<TMessageHandler>() where TMessageHandler : IMessageHandler;

        IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler(Type messageHandlerType);

        IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandlers(params Type[] messageHandlerTypes);

        IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandlers(params Assembly[] assembly);

        IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler(IMessageHandler messageHandler);

        IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler<TMessageHandler>(Func<TMessageHandler> messageHandler) where TMessageHandler : class, IMessageHandler<TMessage>;
    }

    public static class DefaultMessageHandlerSelectorMessageDispatcherConfigurationExtensions
    {
        public static IDefaultMessageHandlerSelectorFactoryConfiguration<TMessage> WithDefaultMessageHandlerSelectorFactory<TMessage>(this IMessageDispatcherConfiguration<TMessage> messageDispatcherConfiguration)
            where TMessage : class
        {
            var simpleMessageHandlerSelector = new DefaultMessageHandlerSelector();

            messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => simpleMessageHandlerSelector);

            return new DefaultMessageHandlerSelectorFactoryConfiguration<TMessage>(simpleMessageHandlerSelector);
        }
    }
}
