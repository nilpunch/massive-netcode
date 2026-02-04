using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Massive.Netcode
{
	public abstract class Connection
	{
		private static readonly byte[] CopyBuffer = new byte[4096];
		private static readonly int ShiftBytesThreshold = 1024 * 8;

		public NetworkStream Outgoing { get; protected set; }
		public MemoryStream Incoming { get; } = new MemoryStream();

		public int Channel { get; set; }

		public abstract bool IsConnected { get; }

		public bool HasUnreadPayload => IncomingPayloadLength > 0;

		public int IncomingPayloadLength => (int)(Incoming.Length - Incoming.Position);

		public void PopulateIncoming()
		{
			if (!IsConnected)
			{
				return;
			}

			try
			{
				while (Outgoing.DataAvailable)
				{
					var read = Outgoing.Read(CopyBuffer, 0, CopyBuffer.Length);
					if (read > 0)
					{
						var currentReadPos = Incoming.Position;

						Incoming.Seek(0, SeekOrigin.End);
						Incoming.Write(CopyBuffer, 0, read);

						Incoming.Position = currentReadPos;
					}
				}
			}
			catch (IOException) { Disconnect(); }
		}

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
				System.Buffer.BlockCopy(array, (int)Incoming.Position, array, 0, unreadCount);

				Incoming.SetLength(unreadCount);
				Incoming.Position = 0;
			}
		}

		public abstract void Connect(IPEndPoint endPoint);

		public abstract void Disconnect();
	}
}
