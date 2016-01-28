using System;
using Obvs.Types;

namespace Obvs.MessageDispatcher
{
    public interface IMessageDispatcher<TMessage>
	{
		IObservable<MessageDispatchResult<TMessage>> Run(IObservable<TMessage> messages);
	}

	public class MessageDispatchResult<TMessage>
	{
		public MessageDispatchResult(TMessage message, bool handled)
		{
			this.Message = message;
			this.Handled = handled;
		}

		public TMessage Message
		{
			get;
			private set;
		}

		public bool Handled
		{
			get;
			private set;
		}
	}
}
