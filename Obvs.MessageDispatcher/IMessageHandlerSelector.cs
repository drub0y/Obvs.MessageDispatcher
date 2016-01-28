namespace Obvs.MessageDispatcher
{
    public interface IMessageHandlerSelector
	{
        IMessageHandler<TMessage> SelectMessageHandler<TMessage>(TMessage message);
	}
}
