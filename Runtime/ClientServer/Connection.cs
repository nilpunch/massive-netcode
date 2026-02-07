using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Massive.Netcode
{
	public abstract class Connection
	{
		private static readonly byte[] CopyBuffer = new byte[4096];
		private static readonly int ShiftBytesThreshold = 1024 * 8;

		public NetworkStream Stream { get; protected set; }
		public MemoryStream Incoming { get; } = new MemoryStream();
		public MemoryStream Outgoing { get; } = new MemoryStream();

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
				while (Stream.DataAvailable)
				{
					var read = Stream.Read(CopyBuffer, 0, CopyBuffer.Length);
					if (read > 0)
					{
						var currentReadPos = Incoming.Position;

						Incoming.Seek(0, SeekOrigin.End);
						Incoming.Write(CopyBuffer, 0, read);

						Incoming.Position = currentReadPos;
					}
				}
			}
			catch (Exception) { Disconnect(); }
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

		public void FlushOutgoing()
		{
			// FlushOutgoingSlow();
			// return;
			
			if (!IsConnected)
			{
				return;
			}

			try
			{
				Stream.Write(Outgoing.GetBuffer(), 0, (int)Outgoing.Length);
			}
			catch (Exception) { Disconnect(); }

			Outgoing.SetLength(0);
			Outgoing.Position = 0;
		}
		
		public async void FlushOutgoingSlow(int delayMs = 30)
		{
			if (!IsConnected)
			{
				return;
			}

			var payload = new byte[Outgoing.Length];
			Array.Copy(Outgoing.GetBuffer(), payload, (int)Outgoing.Length);
			Outgoing.SetLength(0);
			Outgoing.Position = 0;

			await Task.Delay(delayMs);

			try
			{
				Stream.Write(payload, 0, payload.Length);
			}
			catch (Exception) { Disconnect(); }
		}

		public abstract void Connect(IPEndPoint endPoint);

		public abstract void Disconnect();
	}
}
