using System;
using System.IO;
using System.Net;

namespace Massive.Netcode
{
	public abstract class Connection
	{
		protected static readonly byte[] CopyBuffer = new byte[4096];
		private static readonly int ShiftBytesThreshold = 1024 * 8;

		public MemoryStream Incoming { get; } = new MemoryStream();
		public MemoryStream Outgoing { get; } = new MemoryStream();

		public int Channel { get; set; }

		public abstract bool IsConnected { get; }

		public bool HasUnreadPayload => IncomingPayloadLength > 0;

		public int IncomingPayloadLength => (int)(Incoming.Length - Incoming.Position);

		public abstract void PollIncoming();

		public abstract void PushOutgoing();

		public void CompactIncoming()
		{
			var unreadCount = (int)(Incoming.Length - Incoming.Position);

			if (unreadCount == 0)
			{
				Incoming.SetLength(0);
				Incoming.Position = 0;
			}
			else if (Incoming.Position > ShiftBytesThreshold)
			{
				var array = Incoming.GetBuffer();
				Buffer.BlockCopy(array, (int)Incoming.Position, array, 0, unreadCount);

				Incoming.SetLength(unreadCount);
				Incoming.Position = 0;
			}
		}

		public abstract void Connect(IPEndPoint endPoint);

		public abstract void Disconnect();
	}
}
