using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Massive.Netcode
{
	public class Connection
	{
		private static readonly Stack<Connection> _pool = new Stack<Connection>();
		private static readonly byte[] _copyBuffer = new byte[4096];
		private const int _shiftBytesThreshold = 1024 * 8;

		public NetworkStream Outgoing;
		public readonly MemoryStream Incoming = new MemoryStream();
		public int Channel;
		public bool IsBad;

		public bool HasUnreadPayload => IncomingPayloadLength > 0;

		public int IncomingPayloadLength => (int)(Incoming.Length - Incoming.Position);

		public void PopulateIncoming()
		{
			if (IsBad)
			{
				return;
			}

			try
			{
				while (Outgoing.DataAvailable)
				{
					var read = Outgoing.Read(_copyBuffer, 0, _copyBuffer.Length);
					if (read > 0)
					{
						var currentReadPos = Incoming.Position;

						Incoming.Seek(0, SeekOrigin.End);
						Incoming.Write(_copyBuffer, 0, read);

						Incoming.Position = currentReadPos;
					}
				}
			}
			catch (IOException) { IsBad = true; }
		}

		public void CompactIncoming()
		{
			var unreadCount = (int)(Incoming.Length - Incoming.Position);

			if (unreadCount == 0)
			{
				Incoming.SetLength(0);
				Incoming.Position = 0;
			}
			else if (Incoming.Position > _shiftBytesThreshold)
			{
				var array = Incoming.GetBuffer();
				System.Buffer.BlockCopy(array, (int)Incoming.Position, array, 0, unreadCount);

				Incoming.SetLength(unreadCount);
				Incoming.Position = 0;
			}
		}

		public static Connection Rent(NetworkStream networkStream)
		{
			var connection = _pool.Count > 0 ? _pool.Pop() : new Connection();

			connection.Outgoing = networkStream;
			connection.Incoming.SetLength(0);
			connection.Incoming.Position = 0;

			return connection;
		}

		public static void Return(Connection connection)
		{
			_pool.Push(connection);
		}
	}
}
