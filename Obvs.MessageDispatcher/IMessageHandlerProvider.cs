using Obvs.Types;

namespace Obvs.MessageDispatcher
{
	public interface IMessageHandlerProvider
	{
		IMessageHandler<TMessage> Provide<TMessage>() where TMessage : IMessage;
	}
}
