using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Massive.Netcode
{
	public class TcpConnection : Connection
	{
		private TcpClient TcpClient { get; set; }

		public override bool IsConnected => TcpClient != null && TcpClient.Connected;
		public NetworkStream Stream { get; protected set; }

		public void Init(TcpClient tcpClient)
		{
			TcpClient = tcpClient;
			Stream = tcpClient.GetStream();
			Incoming.SetLength(0);
			Incoming.Position = 0;
			Outgoing.SetLength(0);
			Outgoing.Position = 0;
		}

		public override void Connect(IPEndPoint endPoint)
		{
			TcpClient?.Close();
			TcpClient?.Dispose();

			TcpClient = new TcpClient();
			TcpClient.NoDelay = true;
			TcpClient.Connect(endPoint);

			Stream = TcpClient.GetStream();

			Incoming.SetLength(0);
			Incoming.Position = 0;

			Outgoing.SetLength(0);
			Outgoing.Position = 0;
		}

		public override void Disconnect()
		{
			TcpClient?.Close();
			TcpClient?.Dispose();
			TcpClient = null;
		}

		public override void PollIncoming()
		{
			if (!IsConnected || !Stream.CanRead)
			{
				return;
			}

			try
			{
				while (Stream.DataAvailable)
				{
					var read = Stream.Read(CopyBuffer, 0, CopyBuffer.Length);

					// Graceful disconnect.
					if (read == 0)
					{
						Disconnect();
						return;
					}

					var currentReadPos = Incoming.Position;
					Incoming.Seek(0, SeekOrigin.End);
					Incoming.Write(CopyBuffer, 0, read);
					Incoming.Position = currentReadPos;
				}
			}
			catch (Exception ex)
			{
				HandleNetworkException(ex);
			}
		}

		public override void PushOutgoing()
		{
			if (!IsConnected || !Stream.CanWrite || Outgoing.Length == 0)
			{
				return;
			}

			try
			{
				Stream.Write(Outgoing.GetBuffer(), 0, (int)Outgoing.Length);
				Outgoing.SetLength(0);
				Outgoing.Position = 0;
			}
			catch (Exception ex)
			{
				HandleNetworkException(ex);
			}
		}

		public void HandleNetworkException(Exception exception)
		{
			var socketException = exception as SocketException ?? exception.InnerException as SocketException;

			if (socketException != null)
			{
				switch (socketException.SocketErrorCode)
				{
					case SocketError.ConnectionReset:
					case SocketError.ConnectionAborted:
					case SocketError.Shutdown:
					case SocketError.Disconnecting:
					case SocketError.NotConnected:
						Disconnect();
						break;
				}
			}
			else if (exception is ObjectDisposedException)
			{
				Disconnect();
			}
			else
			{
				throw exception;
			}
		}
	}
}
