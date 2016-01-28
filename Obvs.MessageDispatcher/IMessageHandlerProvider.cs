namespace Obvs.MessageDispatcher
{
    public interface IMessageHandlerProvider
	{
        IMessageHandler<TMessage> GetMessageHandler<TMessage>();
	}
}
