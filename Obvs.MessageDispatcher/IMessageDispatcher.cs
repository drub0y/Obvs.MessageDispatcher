using Obvs.Types;
using System;
using System.Threading;

namespace Obvs.MessageDispatcher
{
	public interface IMessageDispatcher
	{
		IObservable<MessageDispatchResult> Run(IObservable<IMessage> messages, CancellationToken cancellationToken);
	}

	public class MessageDispatchResult
	{
		public MessageDispatchResult(IMessage message, bool handled)
		{
			this.Message = message;
			this.Handled = handled;
		}

		public IMessage Message
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
