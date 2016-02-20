using System;

namespace Obvs.MessageDispatcher
{
    internal interface ISimpleMessageHandlerSelector : IMessageHandlerSelector
    {
        void RegisterMessageHandler(Type messageHandlerType);
        void RegisterMessageHandler(IMessageHandler messageHandler);
        void RegisterMessageHandler<TMessageHandler>(Func<TMessageHandler> messageHandlerFactory) where TMessageHandler : class;
    }
}